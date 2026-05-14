using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StepStyle.Web.Data;
using StepStyle.Web.Models;
using System;
using System.Threading.Tasks;

namespace StepStyle.Web.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ReviewController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> Add(int ProductId, int Rating, string Text)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var review = new Review
            {
                ProductId = ProductId,
                UserId = user.Id,
                Rating = Rating,
                Text = Text,
                DatePosted = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Дякуємо! Ваш відгук успішно додано.";
            return RedirectToAction("Details", "Catalog", new { id = ProductId });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReplyToReview(int ReviewId, string ReplyText)
        {
            var review = await _context.Reviews.FindAsync(ReviewId);

            if (review != null && !string.IsNullOrWhiteSpace(ReplyText))
            {
                var userText = review.Text.Split("|ADMIN_REPLY|")[0].Trim();
                review.Text = $"{userText} |ADMIN_REPLY| {ReplyText}";

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Відповідь магазину збережена!";
            }

            return Redirect(Request.Headers["Referer"].ToString());
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Відгук видалено.";
            }
            return Redirect(Request.Headers["Referer"].ToString());
        }
    }
}