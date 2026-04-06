using Microsoft.AspNetCore.Mvc;
using StepStyle.Web.Models;
using StepStyle.Web.Repositories.Interfaces;
using System.Threading.Tasks;

namespace StepStyle.Web.Controllers
{
    public class BrandController : Controller
    {
        private readonly IGenericRepository<Brand> _brandRepository;

        public BrandController(IGenericRepository<Brand> brandRepository)
        {
            _brandRepository = brandRepository;
        }

        public async Task<IActionResult> Index()
        {
            var brands = await _brandRepository.GetAllIncludeAsync(b => b.Products);
            return View(brands);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(Brand brand)
        {
            if (ModelState.IsValid)
            {
                await _brandRepository.AddAsync(brand);
                return RedirectToAction("Index");
            }
            return View(brand);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var brand = await _brandRepository.GetByIdAsync(id);
            if (brand == null) return NotFound();
            return View(brand);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Brand brand)
        {
            if (ModelState.IsValid)
            {
                await _brandRepository.UpdateAsync(brand);
                return RedirectToAction("Index");
            }
            return View(brand);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var brand = await _brandRepository.GetByIdAsync(id);
            if (brand == null) return NotFound();
            return View(brand);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _brandRepository.DeleteAsync(id);
            return RedirectToAction("Index");
        }
    }
}