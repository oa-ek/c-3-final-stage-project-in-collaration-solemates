namespace StepStyle.Web.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public decimal PriceAtPurchase { get; set; } 
        public int Quantity { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }

        public int ProductVariantId { get; set; }
        public ProductVariant ProductVariant { get; set; }
    }
}