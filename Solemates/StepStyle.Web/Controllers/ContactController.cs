using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StepStyle.Web.Data;
using StepStyle.Web.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StepStyle.Web.Controllers
{
    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ContactController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = new ContactMessage();

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
                    model.Email = user.Email;
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ContactMessage model)
        {
            if (ModelState.IsValid)
            {
                model.CreatedAt = DateTime.UtcNow;
                _context.ContactMessages.Add(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Дякуємо! Ваше повідомлення успішно надіслано. Ми зв'яжемося з вами найближчим часом.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MyMessages()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || string.IsNullOrEmpty(user.Email)) return Challenge();

            var messages = await _context.ContactMessages
                .Where(m => m.Email == user.Email)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            return View(messages);
        }
    }
}