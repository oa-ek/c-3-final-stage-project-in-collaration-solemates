using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace StepStyle.Web.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public string ShippingAddress { get; set; } = string.Empty;
        public string DeliveryMethod { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;

        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

        public string? UserId { get; set; }
        public IdentityUser? User { get; set; }

        public List<OrderItem> OrderItems { get; set; } = new();
    }
}