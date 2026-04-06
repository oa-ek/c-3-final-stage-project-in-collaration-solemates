using Microsoft.AspNetCore.Mvc;
using StepStyle.Web.Models;
using StepStyle.Web.Repositories.Interfaces;
using System.Threading.Tasks;

namespace StepStyle.Web.Controllers
{
    public class BrandController : Controller
    {
        // Підключаємо вашу базу даних (репозиторій)
        private readonly IGenericRepository<Brand> _brandRepository;

        public BrandController(IGenericRepository<Brand> brandRepository)
        {
            _brandRepository = brandRepository;
        }

        // 1. ПОКАЗАТИ ВСІ БРЕНДИ (тепер беремо їх з бази даних)
        public async Task<IActionResult> Index()
        {
            var brands = await _brandRepository.GetAllAsync();
            return View(brands);
        }

        // 2. ВІДКРИТИ ФОРМУ СТВОРЕННЯ (порожню)
        public IActionResult Create()
        {
            return View();
        }

        // 3. ЗБЕРЕГТИ НОВИЙ БРЕНД У БАЗУ (коли натиснули кнопку "Зберегти")
        [HttpPost]
        public async Task<IActionResult> Create(Brand brand)
        {
            if (ModelState.IsValid)
            {
                await _brandRepository.AddAsync(brand); // Зберігаємо
                return RedirectToAction("Index"); // Повертаємось до списку
            }
            return View(brand); // Якщо помилка, залишаємось на формі
        }
    }
}