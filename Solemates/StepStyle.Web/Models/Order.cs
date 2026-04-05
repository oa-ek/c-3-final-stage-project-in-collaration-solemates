using Microsoft.AspNetCore.Identity;

namespace StepStyle.Web.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }

        public string UserId { get; set; }
        public IdentityUser User { get; set; }

        public List<OrderItem> OrderItems { get; set; } = new();
    }
}