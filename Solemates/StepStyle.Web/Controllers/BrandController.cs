using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StepStyle.Web.Data;
using StepStyle.Web.Models;
using System.Linq;
using System.Threading.Tasks;

namespace StepStyle.Web.Controllers
{
    public class BrandController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BrandController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var brands = await _context.Brands
                .Include(b => b.Products)
                    .ThenInclude(p => p.Images)
                .ToListAsync();

            return View(brands);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(Brand brand)
        {
            if (ModelState.IsValid)
            {
                _context.Brands.Add(brand);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(brand);
        }

        public async Task<IActionResult> Details(int id)
        {
            var brand = await _context.Brands
                .Include(b => b.Products)
                    .ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (brand == null) return NotFound();

            return View(brand);
        }
    }
}