using Microsoft.AspNetCore.Identity;

namespace StepStyle.Web.Models
{
    public class Address
    {
        public int Id { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string PostalCode { get; set; }

        public string UserId { get; set; }
        public IdentityUser User { get; set; }
    }
}