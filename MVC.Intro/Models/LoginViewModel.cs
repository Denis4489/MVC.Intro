using System.ComponentModel.DataAnnotations;

namespace MVC.Intro.Models
{
    public class LoginViewModel
    {
        [Display(Name = "Имейл или потребителско име")]
        [Required(ErrorMessage = "Въведете имейл или потребителско име")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Парола")]
        [Required(ErrorMessage = "Въведете парола")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Незадължително. Ако е попълнено, ще се показва в навигацията.
        /// Ако е празно, се извлича автоматично от имейл адреса.
        /// </summary>
        [Display(Name = "Вашето име (по желание)")]
        [StringLength(50, ErrorMessage = "Името може да е максимум 50 символа")]
        public string? DisplayName { get; set; }

        [Display(Name = "Запомни ме")]
        public bool RememberMe { get; set; }
    }
}
