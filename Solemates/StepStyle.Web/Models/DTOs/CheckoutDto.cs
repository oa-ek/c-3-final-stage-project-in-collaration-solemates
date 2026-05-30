using System.ComponentModel.DataAnnotations;

namespace StepStyle.Web.Models.DTOs
{
    public class CheckoutDto
    {
        [Required(ErrorMessage = "Будь ласка, вкажіть адресу доставки або відділення Нової Пошти")]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Будь ласка, оберіть спосіб доставки")]
        public string DeliveryMethod { get; set; } = string.Empty;

        [Required(ErrorMessage = "Будь ласка, оберіть спосіб оплати")]
        public string PaymentMethod { get; set; } = string.Empty;
    }
}