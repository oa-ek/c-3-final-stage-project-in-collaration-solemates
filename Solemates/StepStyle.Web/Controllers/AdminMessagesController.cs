using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StepStyle.Web.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StepStyle.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminMessagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminMessagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var messages = await _context.ContactMessages
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            return View(messages);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var message = await _context.ContactMessages.FindAsync(id);
            if (message != null)
            {
                message.IsRead = true;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Повідомлення позначено як прочитане.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Reply(int id)
        {
            var message = await _context.ContactMessages.FindAsync(id);
            if (message == null) return NotFound();

            return View(message);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(int id, string adminReply)
        {
            var message = await _context.ContactMessages.FindAsync(id);
            if (message == null) return NotFound();

            if (string.IsNullOrWhiteSpace(adminReply))
            {
                ModelState.AddModelError("", "Текст відповіді не може бути порожнім.");
                return View(message);
            }

            message.AdminReply = adminReply;
            message.RepliedAt = DateTime.UtcNow;
            message.IsRead = true; 

            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] = $"Відповідь успішно збережено для {message.Email}.";
            return RedirectToAction(nameof(Index));
        }
    }
}