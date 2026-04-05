using Microsoft.AspNetCore.Identity;

namespace StepStyle.Web.Models
{
    public class Cart
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public IdentityUser User { get; set; }

        public List<CartItem> CartItems { get; set; } = new();
    }
}