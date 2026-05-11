using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StepStyle.Web.Data;
using StepStyle.Web.Services.ExternalApi;
using System.Linq;
using System.Threading.Tasks;

namespace StepStyle.Web.Controllers
{
    [AllowAnonymous]
    public class CatalogController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IExchangeRateService _exchangeRateService;
        private readonly IPexelsService _pexelsService;
        private readonly ICountryService _countryService;

        public CatalogController(
            ApplicationDbContext context,
            IExchangeRateService exchangeRateService,
            IPexelsService pexelsService,
            ICountryService countryService)
        {
            _context = context;
            _exchangeRateService = exchangeRateService;
            _pexelsService = pexelsService;
            _countryService = countryService;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .ToListAsync();

            return View(products);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.Size)
                .Include(p => p.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            if (!string.IsNullOrEmpty(product.Brand?.Country))
            {
                ViewBag.CountryInfo = await _countryService.GetCountryByNameAsync(product.Brand.Country);
            }

            var rates = await _exchangeRateService.GetExchangeRatesAsync();
            if (rates != null)
            {
                var usdRate = rates.FirstOrDefault(r => r.Cc == "USD")?.Rate ?? 0m;
                var eurRate = rates.FirstOrDefault(r => r.Cc == "EUR")?.Rate ?? 0m;

                if (usdRate > 0m) ViewBag.UsdPrice = (product.Price / usdRate).ToString("F2");
                if (eurRate > 0m) ViewBag.EurPrice = (product.Price / eurRate).ToString("F2");
            }

            string searchQuery = $"{product.Brand?.Name} {product.Name}".Trim();
            if (string.IsNullOrEmpty(searchQuery)) searchQuery = "sneakers";

            ViewBag.PexelsPhotos = await _pexelsService.SearchPhotosAsync(searchQuery, 3);

            return View(product);
        }
    }
}