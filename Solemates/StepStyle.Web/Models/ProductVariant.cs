namespace StepStyle.Web.Models
{
    public class ProductVariant
    {
        public int Id { get; set; }
        public int QuantityInStock { get; set; } 

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int SizeId { get; set; }
        public Size Size { get; set; }
    }
}