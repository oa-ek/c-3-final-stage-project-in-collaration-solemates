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

        public ProductController(
            IGenericRepository<Product> productRepository,
            IGenericRepository<Brand> brandRepository,
            IGenericRepository<Size> sizeRepository,
            IGenericRepository<Category> categoryRepository,
            IGenericRepository<ProductVariant> variantRepository)
        {
            _productRepository = productRepository;
            _brandRepository = brandRepository;
            _sizeRepository = sizeRepository;
            _categoryRepository = categoryRepository;
            _variantRepository = variantRepository;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var products = await _productRepository.GetAllIncludeAsync(
                p => p.Images,
                p => p.Brand
            );

            var product = products.FirstOrDefault(p => p.Id == id);

            if (product == null) return NotFound();

            var allVariants = await _variantRepository.GetAllIncludeAsync(v => v.Size);
            product.Variants = allVariants.Where(v => v.ProductId == id).ToList();

            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int brandId)
        {
            var brand = await _brandRepository.GetByIdAsync(brandId);
            if (brand == null) return NotFound();
            var existingSizes = await _sizeRepository.GetAllAsync();

            if (existingSizes.Count() < 10)
            {
                for (int i = 18; i <= 45; i++)
                {
                    if (!existingSizes.Any(s => s.Value == i.ToString()))
                    {
                        await _sizeRepository.AddAsync(new Size { Value = i.ToString() });
                    }
                }
                existingSizes = await _sizeRepository.GetAllAsync();
            }

            var categories = await _categoryRepository.GetAllAsync();

            var product = new Product
            {
                BrandId = brandId,
                CategoryId = categories.FirstOrDefault()?.Id ?? 1,
                Description = "",
                Gender = Gender.Unisex
            };

            await PrepareViewBag(brandId);

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile uploadedImage)
        {
            if (product.Variants == null)
                product.Variants = new List<ProductVariant>();

            CleanProductModelState(product);

            if (ModelState.IsValid)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(product.SKU))
                        product.SKU = "ART-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();

                    if (uploadedImage != null && uploadedImage.Length > 0)
                    {
                        product.Images = new List<ProductImage> { await SaveImage(uploadedImage) };
                    }

                    var validVariants = product.Variants
                        .Where(v => v != null && v.QuantityInStock > 0 && v.SizeId > 0)
                        .ToList();

                    product.Variants = null;

                    await _productRepository.AddAsync(product);
                    foreach (var variant in validVariants)
                    {
                        variant.ProductId = product.Id;
                        variant.Id = 0;
                        await _variantRepository.AddAsync(variant);
                    }

                    return RedirectToAction("Details", "Brand", new { id = product.BrandId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Помилка бази: " + ex.Message);
                }
            }

            await PrepareViewBag(product.BrandId);
            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();

            await PrepareViewBag(product.BrandId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product, IFormFile? uploadedImage, int? ExistingImageId, string? ExistingImageUrl)
        {
            if (product.Variants == null)
                product.Variants = new List<ProductVariant>();

            CleanProductModelState(product);

            if (ModelState.IsValid)
            {
                try
                {
                    var allVariants = await _variantRepository.GetAllAsync();
                    var oldVariants = allVariants.Where(v => v.ProductId == product.Id).ToList();

                    foreach (var old in oldVariants)
                    {
                        await _variantRepository.DeleteAsync(old.Id);
                    }

                    var validVariants = product.Variants
                        .Where(v => v != null && v.QuantityInStock > 0 && v.SizeId > 0)
                        .ToList();

                    foreach (var variant in validVariants)
                    {
                        variant.ProductId = product.Id;
                        variant.Id = 0;
                        await _variantRepository.AddAsync(variant);
                    }

                    if (uploadedImage != null && uploadedImage.Length > 0)
                    {
                        product.Images = new List<ProductImage> { await SaveImage(uploadedImage) };
                    }
                    else if (!string.IsNullOrEmpty(ExistingImageUrl))
                    {
                        product.Images = new List<ProductImage>
                        {
                            new ProductImage
                            {
                                Id = ExistingImageId ?? 0,
                                ImageUrl = ExistingImageUrl,
                                IsMain = true,
                                ProductId = product.Id
                            }
                        };
                    }

                    var currentVariants = product.Variants;
                    product.Variants = null;

                    await _productRepository.UpdateAsync(product);
                    return RedirectToAction("Details", "Brand", new { id = product.BrandId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Помилка оновлення: " + ex.Message);
                }
            }
            await PrepareViewBag(product.BrandId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();

            int brandId = product.BrandId;

            try
            {
                var allVariants = await _variantRepository.GetAllAsync();
                var productVariants = allVariants.Where(v => v.ProductId == id).ToList();

                foreach (var variant in productVariants)
                {
                    await _variantRepository.DeleteAsync(variant.Id);
                }

                await _productRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Помилка при видаленні: " + ex.Message);
                return RedirectToAction("Details", "Brand", new { id = brandId });
            }

            return RedirectToAction("Details", "Brand", new { id = brandId });
        }

        private void CleanProductModelState(Product product)
        {
            ModelState.Remove("Brand");
            ModelState.Remove("Category");
            ModelState.Remove("Images");
            ModelState.Remove("Reviews");
            ModelState.Remove("SKU");

            var keysToRemove = ModelState.Keys.Where(k => k.StartsWith("Variants")).ToList();
            foreach (var key in keysToRemove)
            {
                ModelState.Remove(key);
            }
        }

        private async Task PrepareViewBag(int brandId)
        {
            var brand = await _brandRepository.GetByIdAsync(brandId);
            ViewBag.BrandName = brand?.Name;
            var sizes = await _sizeRepository.GetAllAsync();
            ViewBag.FullSizesList = sizes.OrderBy(s => double.Parse(s.Value.Replace(',', '.'))).ToList();
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

            return new ProductImage
            {
                ImageUrl = "/images/products/" + fileName,
                IsMain = true
            };
        }
    }
}