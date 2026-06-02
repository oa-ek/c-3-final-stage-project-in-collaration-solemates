using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StepStyle.Web.Models;
using StepStyle.Web.Repositories.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using StepStyle.Web.Services.ExternalApi;

namespace StepStyle.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly IGenericRepository<Product> _productRepository;
        private readonly IGenericRepository<Brand> _brandRepository;
        private readonly IGenericRepository<Size> _sizeRepository;
        private readonly IGenericRepository<Category> _categoryRepository;
        private readonly IGenericRepository<ProductVariant> _variantRepository;
        private readonly IExchangeRateService _exchangeRateService;
        private readonly IPexelsService _pexelsService;
        private readonly ICountryService _countryService;

        public ProductController(
            IGenericRepository<Product> productRepository,
            IGenericRepository<Brand> brandRepository,
            IGenericRepository<Size> sizeRepository,
            IGenericRepository<Category> categoryRepository,
            IGenericRepository<ProductVariant> variantRepository,
            IExchangeRateService exchangeRateService,
            IPexelsService pexelsService,
            ICountryService countryService)
        {
            _productRepository = productRepository;
            _brandRepository = brandRepository;
            _sizeRepository = sizeRepository;
            _categoryRepository = categoryRepository;
            _variantRepository = variantRepository;
            _exchangeRateService = exchangeRateService;
            _pexelsService = pexelsService;
            _countryService = countryService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var products = await _productRepository.GetAllIncludeAsync(
                p => p.Images,
                p => p.Brand,
                p => p.Reviews
            );

            var product = products.FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();

            if (!string.IsNullOrEmpty(product.Brand?.Country))
            {
                ViewBag.CountryInfo = await _countryService.GetCountryByNameAsync(product.Brand.Country);
            }

            var allVariants = await _variantRepository.GetAllIncludeAsync(v => v.Size);
            product.Variants = allVariants.Where(v => v.ProductId == id).ToList();

            var rates = await _exchangeRateService.GetExchangeRatesAsync();
            var usdRate = rates?.FirstOrDefault(r => r.Cc == "USD")?.Rate ?? 0m;
            var eurRate = rates?.FirstOrDefault(r => r.Cc == "EUR")?.Rate ?? 0m;

            if (usdRate > 0m) ViewBag.UsdPrice = (product.Price / usdRate).ToString("F2");
            if (eurRate > 0m) ViewBag.EurPrice = (product.Price / eurRate).ToString("F2");

            ViewBag.PexelsPhotos = await _pexelsService.SearchPhotosAsync(product.Name ?? "sneakers", 3);

            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int brandId)
        {
            var brand = await _brandRepository.GetByIdAsync(brandId);
            if (brand == null) return NotFound();
            await EnsureSizesExist();
            await EnsureCategoriesExist();

            var categories = await _categoryRepository.GetAllAsync();

            var product = new Product
            {
                BrandId = brandId,
                CategoryId = categories.FirstOrDefault()?.Id ?? 1,
                Description = "",
                Gender = Gender.Unisex,
                Variants = new List<ProductVariant>()
            };

            await PrepareViewBag(brandId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, List<IFormFile> imageFiles)
        {
            CleanProductModelState(product);

            if (!ModelState.IsValid)
            {
                await PrepareViewBag(product.BrandId);
                return View(product);
            }

            if (string.IsNullOrWhiteSpace(product.SKU))
                product.SKU = "ART-" + Guid.NewGuid().ToString("N")[..8].ToUpper();

            product.Images = new List<ProductImage>();
            if (imageFiles != null && imageFiles.Count > 0)
            {
                for (int i = 0; i < imageFiles.Count; i++)
                {
                    var productImage = await SaveImage(imageFiles[i]);
                    productImage.IsMain = (i == 0);
                    product.Images.Add(productImage);
                }
            }

            var selectedVariants = product.Variants?
                .Where(v => v != null && v.QuantityInStock > 0 && v.SizeId > 0)
                .ToList() ?? new List<ProductVariant>();

            product.Variants = null;

            await _productRepository.AddAsync(product);

            foreach (var variant in selectedVariants)
            {
                variant.ProductId = product.Id;
                variant.Id = 0;
                await _variantRepository.AddAsync(variant);
            }

            return RedirectToAction("Details", "Brand", new { id = product.BrandId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var products = await _productRepository.GetAllIncludeAsync(p => p.Images);
            var product = products.FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();

            var allVariants = await _variantRepository.GetAllAsync();
            product.Variants = allVariants.Where(v => v.ProductId == id).ToList();
            await EnsureCategoriesExist();
            await PrepareViewBag(product.BrandId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product, List<IFormFile> uploadedImages, int mainImageIndex, List<int> ImagesToDelete, int? ExistingMainImageId)
        {
            CleanProductModelState(product);

            if (ModelState.IsValid)
            {
                var productsWithImages = await _productRepository.GetAllIncludeAsync(p => p.Images);
                var existingProduct = productsWithImages.FirstOrDefault(p => p.Id == product.Id);
                if (existingProduct == null) return NotFound();

                var allVariants = await _variantRepository.GetAllAsync();
                var oldVariants = allVariants.Where(v => v.ProductId == product.Id).ToList();
                foreach (var old in oldVariants) await _variantRepository.DeleteAsync(old.Id);

                if (product.Variants != null)
                {
                    var validVariants = product.Variants
                        .Where(v => v != null && v.QuantityInStock > 0 && v.SizeId > 0)
                        .ToList();

                    foreach (var variant in validVariants)
                    {
                        variant.ProductId = product.Id;
                        variant.Id = 0;
                        await _variantRepository.AddAsync(variant);
                    }
                }

                if (ImagesToDelete != null && ImagesToDelete.Any())
                {
                    foreach (var imageId in ImagesToDelete)
                    {
                        var img = existingProduct.Images.FirstOrDefault(i => i.Id == imageId);
                        if (img != null)
                        {
                            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", img.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                            existingProduct.Images.Remove(img);
                        }
                    }
                }

                bool newPhotoSelectedAsMain = (uploadedImages != null && uploadedImages.Count > 0 && mainImageIndex >= 0);

                if (newPhotoSelectedAsMain)
                {
                    foreach (var img in existingProduct.Images) img.IsMain = false;
                }
                else if (ExistingMainImageId.HasValue)
                {
                    foreach (var img in existingProduct.Images)
                        img.IsMain = (img.Id == ExistingMainImageId.Value);
                }

                if (uploadedImages != null && uploadedImages.Count > 0)
                {
                    for (int i = 0; i < uploadedImages.Count; i++)
                    {
                        var image = await SaveImage(uploadedImages[i]);
                        image.IsMain = newPhotoSelectedAsMain ? (i == mainImageIndex) : false;
                        image.ProductId = product.Id;
                        existingProduct.Images.Add(image);
                    }
                }

                if (existingProduct.Images.Any() && !existingProduct.Images.Any(i => i.IsMain))
                {
                    existingProduct.Images.First().IsMain = true;
                }

                existingProduct.Name = product.Name;
                existingProduct.Price = product.Price;
                existingProduct.Description = product.Description;
                existingProduct.SKU = product.SKU;
                existingProduct.Gender = product.Gender;
                existingProduct.CategoryId = product.CategoryId;

                await _productRepository.UpdateAsync(existingProduct);
                return RedirectToAction("Details", "Catalog", new { id = existingProduct.Id });
            }

            await PrepareViewBag(product.BrandId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var products = await _productRepository.GetAllIncludeAsync(p => p.Images);
            var product = products.FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();

            int brandId = product.BrandId;

            foreach (var img in product.Images)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", img.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
            }

            var allVariants = await _variantRepository.GetAllAsync();
            var productVariants = allVariants.Where(v => v.ProductId == id).ToList();

            foreach (var variant in productVariants) await _variantRepository.DeleteAsync(variant.Id);
            await _productRepository.DeleteAsync(id);

            return RedirectToAction("Details", "Brand", new { id = brandId });
        }

        private async Task EnsureSizesExist()
        {
            var existingSizes = await _sizeRepository.GetAllAsync();
            if (existingSizes.Count() < 10)
            {
                for (int i = 18; i <= 45; i++)
                {
                    if (!existingSizes.Any(s => s.Value == i.ToString()))
                        await _sizeRepository.AddAsync(new Size { Value = i.ToString() });
                }
            }
        }

        private async Task EnsureCategoriesExist()
        {
            var existingCategories = await _categoryRepository.GetAllAsync();
            var defaultCategories = new List<string>
            {
                "Кросівки", "Кеди", "Туфлі", "Черевики", "Чоботи",
                "Босоніжки", "Сандалі", "Шльопанці", "Лофери",
                "Мокасини", "Балетки", "Капці"
            };

            foreach (var categoryName in defaultCategories)
            {
                if (!existingCategories.Any(c => c.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase)))
                {
                    await _categoryRepository.AddAsync(new Category { Name = categoryName });
                }
            }
        }

        private void CleanProductModelState(Product product)
        {
            ModelState.Remove("Brand");
            ModelState.Remove("Category");
            ModelState.Remove("Images");
            ModelState.Remove("Reviews");
            ModelState.Remove("Gender");
            ModelState.Remove("Description");
            if (string.IsNullOrEmpty(product.SKU)) ModelState.Remove("SKU");

            var keysToRemove = ModelState.Keys.Where(k => k.StartsWith("Variants")).ToList();
            foreach (var key in keysToRemove) ModelState.Remove(key);
        }

        private async Task PrepareViewBag(int brandId)
        {
            var brand = await _brandRepository.GetByIdAsync(brandId);
            ViewBag.BrandName = brand?.Name;

            var sizes = await _sizeRepository.GetAllAsync();
            ViewBag.FullSizesList = sizes.OrderBy(s => {
                return double.TryParse(s.Value.Replace(',', '.'), out double res) ? res : 0;
            }).ToList();

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.CategoriesList = categories.OrderBy(c => c.Name).ToList();
        }

        private async Task<ProductImage> SaveImage(IFormFile file)
        {
            string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            string path = Path.Combine(folder, fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return new ProductImage { ImageUrl = "/images/products/" + fileName };
        }
    }
}