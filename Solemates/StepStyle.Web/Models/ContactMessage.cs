using System;
using System.ComponentModel.DataAnnotations;

namespace StepStyle.Web.Models
{
    public class ContactMessage
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Будь ласка, введіть ваше ім'я")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Будь ласка, введіть ваш Email")]
        [EmailAddress(ErrorMessage = "Некоректний формат Email")]
        public string Email { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Subject { get; set; }

        [Required(ErrorMessage = "Будь ласка, введіть повідомлення")]
        public string Message { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;


        public string? AdminReply { get; set; } 

        public DateTime? RepliedAt { get; set; }
    }
}