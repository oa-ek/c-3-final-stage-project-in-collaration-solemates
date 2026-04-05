using Microsoft.AspNetCore.Identity;

namespace StepStyle.Web.Models
{
    public class Review
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int Rating { get; set; } 
        public DateTime DatePosted { get; set; } = DateTime.UtcNow;

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public string UserId { get; set; }
        public IdentityUser User { get; set; }
    }
}