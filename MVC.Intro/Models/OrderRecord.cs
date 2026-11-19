using System;
using System.ComponentModel.DataAnnotations;

namespace MVC.Intro.Models
{
    public class OrderRecord
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Display(Name = "Номер на поръчка")]
        public string OrderNumber { get; set; } = string.Empty;

        [Display(Name = "Дата на поръчка")]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Клиент")]
        public string CustomerName { get; set; } = string.Empty;

        [Display(Name = "Имейл")]
        public string CustomerEmail { get; set; } = string.Empty;

        [Display(Name = "Обща сума")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Брой билети")]
        public int TotalItems { get; set; }

        [Display(Name = "Закупени билети")]
        public string TicketsSummary { get; set; } = string.Empty;
    }
}
