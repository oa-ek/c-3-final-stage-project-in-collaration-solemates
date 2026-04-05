using Microsoft.AspNetCore.Identity;

namespace StepStyle.Web.Models
{
    public class Wishlist
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public IdentityUser User { get; set; }

        public List<WishlistItem> WishlistItems { get; set; } = new();
    }
}