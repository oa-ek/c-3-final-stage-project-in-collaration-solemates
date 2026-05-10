using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StepStyle.Web.Data;
using StepStyle.Web.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using StepStyle.Web.Services.ExternalApi;

namespace StepStyle.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BrandController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IExchangeRateService _exchangeRateService;
        private readonly ICountryService _countryService;

        public BrandController(
            ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment,
            IExchangeRateService exchangeRateService,
            ICountryService countryService)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _exchangeRateService = exchangeRateService;
            _countryService = countryService;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var brands = await _context.Brands
                .Include(b => b.Products)
                    .ThenInclude(p => p.Images)
                .ToListAsync();

            ViewBag.ExchangeRates = await _exchangeRateService.GetExchangeRatesAsync();

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
                return RedirectToAction(nameof(Index));
            }
            return View(brand);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var brand = await _context.Brands
                .Include(b => b.Products)
                    .ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (brand == null) return NotFound();

            ViewBag.CountryInfo = await _countryService.GetCountryByNameAsync("usa");

            return View(brand);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var brand = await _context.Brands.FindAsync(id);
            if (brand == null) return NotFound();

            return View(brand);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Brand brand, IFormFile? logoFile)
        {
            if (id != brand.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingBrand = await _context.Brands.FindAsync(id);
                    if (existingBrand == null) return NotFound();

                    existingBrand.Name = brand.Name;

                    if (logoFile != null && logoFile.Length > 0)
                    {
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "brands");

                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        string uniqueFileName = $"brand_{existingBrand.Id}.jpg";
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await logoFile.CopyToAsync(fileStream);
                        }
                    }

                    _context.Update(existingBrand);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BrandExists(brand.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(brand);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var brand = await _context.Brands.FindAsync(id);
            if (brand == null) return NotFound();

            return View(brand);
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var brand = await _context.Brands
                .Include(b => b.Products)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (brand != null)
            {
                _context.Brands.Remove(brand);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool BrandExists(int id)
        {
            return _context.Brands.Any(e => e.Id == id);
        }
    }
}