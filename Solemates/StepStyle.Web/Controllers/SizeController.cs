using Microsoft.AspNetCore.Mvc;
using StepStyle.Web.Models;
using StepStyle.Web.Repositories.Interfaces;
using System.Threading.Tasks;

namespace StepStyle.Web.Controllers
{
    public class SizeController : Controller
    {
        private readonly IGenericRepository<Size> _sizeRepository;

        public SizeController(IGenericRepository<Size> sizeRepository)
        {
            _sizeRepository = sizeRepository;
        }

        public async Task<IActionResult> Index() => View(await _sizeRepository.GetAllAsync());

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(Size size)
        {
            if (ModelState.IsValid)
            {
                await _sizeRepository.AddAsync(size);
                return RedirectToAction(nameof(Index));
            }
            return View(size);
        }
    }
}