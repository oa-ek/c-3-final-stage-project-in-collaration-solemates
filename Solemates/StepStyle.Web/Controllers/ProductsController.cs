using Microsoft.AspNetCore.Mvc;
using StepStyle.Web.Models;
using StepStyle.Web.Repositories.Interfaces;
using System;
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
                CategoryId = 1,
                Description = "",
                // Вказуємо значення з твого Enum. 
                // Якщо Unisex підкреслює червоним, спробуй Gender.Men або Gender.Other
                Gender = Gender.Unisex
            };

            ViewBag.BrandName = brand.Name;
            return View(product);
        }

        // 2. ЗБЕРЕГТИ НОВИЙ ТОВАР
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            // СТРАХОВКА: Якщо опис або артикул порожні, база SQLite видасть помилку. 
            // Тому ми заповнюємо їх автоматично, якщо користувач їх проігнорував.
            if (string.IsNullOrWhiteSpace(product.Description))
                product.Description = "Опис буде додано пізніше";

            if (string.IsNullOrWhiteSpace(product.SKU))
                product.SKU = "AUTO-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();

            if (product.CategoryId == 0)
                product.CategoryId = 1;

            // Очищаємо ModelState від навігаційних властивостей та полів, які ми заповнили вручну
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
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Помилка бази даних: " + ex.Message);
                }
            }

            var brand = await _brandRepository.GetByIdAsync(product.BrandId);
            ViewBag.BrandName = brand?.Name;
            return View(product);
        }

        // 3. ВІДКРИТИ ФОРМУ РЕДАГУВАННЯ
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();

            var brand = await _brandRepository.GetByIdAsync(product.BrandId);
            ViewBag.BrandName = brand?.Name;

            return View(product);
        }

        // 4. ЗБЕРЕГТИ ЗМІНИ ПРИ РЕДАГУВАННІ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product)
        {
            // При редагуванні також страхуємо SKU та опис
            if (string.IsNullOrWhiteSpace(product.SKU))
                product.SKU = "SKU-UPDATED";

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
                    await _productRepository.UpdateAsync(product);
                    return RedirectToAction("Index", "Brand");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Не вдалося оновити товар: " + ex.Message);
                }
            }

            var brand = await _brandRepository.GetByIdAsync(product.BrandId);
            ViewBag.BrandName = brand?.Name;
            return View(product);
        }

        // 5. ВИДАЛЕННЯ
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();

            await _productRepository.DeleteAsync(id);
            return RedirectToAction("Index", "Brand");
        }
    }
}