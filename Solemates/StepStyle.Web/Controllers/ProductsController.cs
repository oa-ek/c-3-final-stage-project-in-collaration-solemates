using Microsoft.AspNetCore.Mvc;
using StepStyle.Web.Models;
using StepStyle.Web.Repositories.Interfaces;
using System.Threading.Tasks;

namespace StepStyle.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IGenericRepository<Product> _productRepository;
        private readonly IGenericRepository<Brand> _brandRepository;

        public ProductController(IGenericRepository<Product> productRepository, IGenericRepository<Brand> brandRepository)
        {
            _productRepository = productRepository;
            _brandRepository = brandRepository;
        }

        public async Task<IActionResult> Create(int brandId)
        {
            var brand = await _brandRepository.GetByIdAsync(brandId);
            if (brand == null) return NotFound();

            var product = new Product
            {
                BrandId = brandId,
                Description = "Опис з'явиться пізніше",
                SKU = "TEMP-SKU"
            };

            ViewBag.BrandName = brand.Name;
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {

            if (string.IsNullOrWhiteSpace(product.Description))
                product.Description = "Тимчасовий опис";

            if (string.IsNullOrWhiteSpace(product.SKU))
                product.SKU = "AUTO-" + System.Guid.NewGuid().ToString().Substring(0, 5);

            if (product.CategoryId == 0)
                product.CategoryId = 1;

            ModelState.Remove("Brand");
            ModelState.Remove("Category");
            ModelState.Remove("Description");
            ModelState.Remove("SKU");
            ModelState.Remove("Variants");
            ModelState.Remove("Images");
            ModelState.Remove("Reviews");

            if (ModelState.IsValid)
            {
                try
                {
                    await _productRepository.AddAsync(product);
                    return RedirectToAction("Index", "Brand");
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError("", "Помилка бази даних: " + ex.Message);
                }
            }

            var brand = await _brandRepository.GetByIdAsync(product.BrandId);
            ViewBag.BrandName = brand?.Name;
            return View(product);
        }
    }
}