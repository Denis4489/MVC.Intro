using MVC.Intro.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MVC.Intro.Models
{
    public class Event
    {
        [DisplayName("Идентификатор")]
        public Guid Id { get; set; }
        [DisplayName("Наименование")]
        [Required(ErrorMessage = "Полето е задължително")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Дължината на името трябва да е между 3 и 100 символа")]
        [RegularExpression(@"^[A-Za-zА-Яа-яЁёЪъЬь\s\-]+$", ErrorMessage = "Името може да съдържа само букви, интервали и тирета")]
        public required string Name { get; set; }
        [DisplayName("Цена")]
        [Required(ErrorMessage = "Задължително е събитието да има цена")]
        [Range(10.00, 9999.99, ErrorMessage = "Цената трябва да е между 10.00 и 9999.99")]
        public decimal Price { get; set; }
        [DisplayName("Снимка")]
        public string? ImageUrl { get; set; }
        [DisplayName("Дата на събитието")]
        [Required(ErrorMessage = "Полето е задължително")]
        public DateTime EventDate { get; set; }
        [DisplayName("Място")]
        [Required(ErrorMessage = "Полето е задължително")]
        [StringLength(100, ErrorMessage = "Дължината на мястото не трябва да надвишава 100 символа")]
        public required string Venue { get; set; }
        [DisplayName("Тип билет")]
        [Required(ErrorMessage = "Полето е задължително")]
        public required string TicketType { get; set; }
        [DisplayName("Капацитет")]
        [Required(ErrorMessage = "Полето е задължително")]
        [Range(1, 100000, ErrorMessage = "Капацитетът трябва да е между 1 и 100000")]
        public int Capacity { get; set; }
        [DisplayName("Налични билети")]
        [Required(ErrorMessage = "Полето е задължително")]
        public int AvailableTickets { get; set; }
        [DisplayName("Описание")]
        [StringLength(500, ErrorMessage = "Дължината на описанието не трябва да надвишава 500 символа")]
        public string? Description { get; set; }
    }
}
