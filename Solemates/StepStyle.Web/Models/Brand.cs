using Microsoft.CodeAnalysis;

namespace StepStyle.Web.Models
{
    public class Brand
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }

        public List<Product> Products { get; set; } = new();
    }
}