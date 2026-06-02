using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StepStyle.Web.Data;
using StepStyle.Web.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StepStyle.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminOrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? paymentStatus, string? orderStatus, DateTime? orderDate, string? searchId)
        {
            var ordersQuery = _context.Orders
                .Include(o => o.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchId))
            {
                if (int.TryParse(searchId.Trim().Replace("#", ""), out int parsedId))
                {
                    ordersQuery = ordersQuery.Where(o => o.Id == parsedId);
                }
                else
                {
                    ordersQuery = ordersQuery.Where(o => false);
                }
            }
            else
            {

                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    if (Enum.TryParse(typeof(PaymentStatus), paymentStatus, out var parsedPayment))
                    {
                        ordersQuery = ordersQuery.Where(o => o.PaymentStatus == (PaymentStatus)parsedPayment);
                    }
                }

                if (!string.IsNullOrEmpty(orderStatus))
                {
                    if (Enum.TryParse(typeof(OrderStatus), orderStatus, out var parsedOrder))
                    {
                        ordersQuery = ordersQuery.Where(o => o.Status == (OrderStatus)parsedOrder);
                    }
                }

                if (orderDate.HasValue)
                {
                    var targetDate = orderDate.Value.Date;
                    ordersQuery = ordersQuery.Where(o => o.OrderDate >= targetDate && o.OrderDate < targetDate.AddDays(1));
                }
            }

            var orders = await ordersQuery
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            ViewBag.CurrentPaymentStatus = paymentStatus;
            ViewBag.CurrentOrderStatus = orderStatus;
            ViewBag.CurrentOrderDate = orderDate?.ToString("yyyy-MM-dd");
            ViewBag.CurrentSearchId = searchId; 

            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant!)
                        .ThenInclude(pv => pv.Product)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant!)
                        .ThenInclude(pv => pv.Size)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status, PaymentStatus paymentStatus)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            if (status == OrderStatus.Cancelled && order.Status != OrderStatus.Cancelled)
            {
                foreach (var item in order.OrderItems)
                {
                    if (item.ProductVariant != null)
                    {
                        item.ProductVariant.QuantityInStock += item.Quantity;
                    }
                }
                TempData["SuccessMessage"] = "Замовлення скасовано, товари повернуто на склад!";
            }
            else if (order.Status == OrderStatus.Cancelled && status != OrderStatus.Cancelled)
            {
                foreach (var item in order.OrderItems)
                {
                    if (item.ProductVariant != null)
                    {
                        item.ProductVariant.QuantityInStock -= item.Quantity;
                    }
                }
                TempData["SuccessMessage"] = "Замовлення відновлено, товари знову зарезервовані на складі.";
            }
            else
            {
                TempData["SuccessMessage"] = "Статуси замовлення успішно оновлено!";
            }

            order.Status = status;
            order.PaymentStatus = paymentStatus;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = order.Id });
        }
    }
}