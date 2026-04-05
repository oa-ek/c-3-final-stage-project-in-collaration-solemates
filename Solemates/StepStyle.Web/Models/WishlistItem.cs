namespace StepStyle.Web.Models
{
    public class WishlistItem
    {
        public int Id { get; set; }
        public DateTime AddedDate { get; set; } = DateTime.UtcNow;

        public int WishlistId { get; set; }
        public Wishlist Wishlist { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}