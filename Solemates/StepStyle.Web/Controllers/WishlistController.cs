using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StepStyle.Web.Data;
using StepStyle.Web.Models;

namespace StepStyle.Web.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public WishlistController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var wishlist = await _context.Wishlists
                .Include(w => w.WishlistItems)
                    .ThenInclude(wi => wi.Product)
                        .ThenInclude(p => p.Images)
                .Include(w => w.WishlistItems)
                    .ThenInclude(wi => wi.Product)
                        .ThenInclude(p => p.Variants) 
                            .ThenInclude(v => v.Size)
                .FirstOrDefaultAsync(w => w.UserId == userId);

            var items = wishlist?.WishlistItems.ToList() ?? new List<WishlistItem>();

            return View(items);
        }

        [HttpPost]
        public async Task<IActionResult> Add(int productId)
        {
            var userId = _userManager.GetUserId(User);
            var wishlist = await _context.Wishlists
                .Include(w => w.WishlistItems)
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (wishlist == null)
            {
                wishlist = new Wishlist { UserId = userId };
                _context.Wishlists.Add(wishlist);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("FOREIGN KEY") == true)
                {
                    TempData["ErrorMessage"] = "Сесія застаріла. Будь ласка, вийдіть з акаунту та увійдіть знову.";
                    return RedirectToAction(nameof(Index));
                }
            }

            if (!wishlist.WishlistItems.Any(wi => wi.ProductId == productId))
            {
                var item = new WishlistItem
                {
                    WishlistId = wishlist.Id,
                    ProductId = productId,
                    AddedDate = DateTime.UtcNow
                };

                _context.WishlistItems.Add(item);

                try
                {
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Товар додано до списку бажань!";
                }
                catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("FOREIGN KEY") == true)
                {
                    TempData["ErrorMessage"] = "Не вдалося додати товар. Можливо, його вже видалено з бази.";
                }
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int productId)
        {
            var userId = _userManager.GetUserId(User);
            var item = await _context.WishlistItems
                .FirstOrDefaultAsync(wi => wi.ProductId == productId && wi.Wishlist.UserId == userId);

            if (item != null)
            {
                _context.WishlistItems.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> MoveToCart(int productId)
        {
            var userId = _userManager.GetUserId(User);

            var wishlistItem = await _context.WishlistItems
                .Include(wi => wi.Product)
                    .ThenInclude(p => p.Variants)
                .FirstOrDefaultAsync(wi => wi.ProductId == productId && wi.Wishlist.UserId == userId);

            if (wishlistItem == null) return NotFound();

            var variant = wishlistItem.Product.Variants.FirstOrDefault(v => v.QuantityInStock > 0);

            if (variant == null)
            {
                TempData["ErrorMessage"] = "Цього товару зараз немає в наявності.";
                return RedirectToAction(nameof(Index));
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId) ?? new Cart { UserId = userId };

            if (cart.Id == 0)
            {
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.ProductVariantId == variant.Id);

            if (cartItem == null)
            {
                _context.CartItems.Add(new CartItem
                {
                    CartId = cart.Id,
                    ProductVariantId = variant.Id,
                    Quantity = 1
                });
            }
            else
            {
                cartItem.Quantity++;
            }

            _context.WishlistItems.Remove(wishlistItem);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Cart");
        }


        [HttpPost]
        public async Task<IActionResult> AddAllToCart()
        {
            var userId = _userManager.GetUserId(User);

            var wishlistItems = await _context.WishlistItems
                .Include(wi => wi.Product)
                    .ThenInclude(p => p.Variants)
                .Where(wi => wi.Wishlist.UserId == userId)
                .ToListAsync();

            if (!wishlistItems.Any())
            {
                return RedirectToAction(nameof(Index));
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            int addedCount = 0;

            foreach (var wishItem in wishlistItems)
            {
                var variant = wishItem.Product.Variants
                    .FirstOrDefault(v => v.QuantityInStock > 0);

                if (variant != null)
                {
                    var existingCartItem = await _context.CartItems
                        .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.ProductVariantId == variant.Id);

                    if (existingCartItem == null)
                    {
                        _context.CartItems.Add(new CartItem
                        {
                            CartId = cart.Id,
                            ProductVariantId = variant.Id,
                            Quantity = 1
                        });
                    }
                    else
                    {
                        existingCartItem.Quantity++;
                    }

                    _context.WishlistItems.Remove(wishItem);
                    addedCount++;
                }
            }

            await _context.SaveChangesAsync();

            if (addedCount > 0)
                TempData["SuccessMessage"] = $"Успішно перенесено товарів у кошик: {addedCount}";
            else
                TempData["ErrorMessage"] = "Не вдалося додати товари (можливо, їх немає в наявності)";

            return RedirectToAction("Index", "Cart");
        }
    }
}