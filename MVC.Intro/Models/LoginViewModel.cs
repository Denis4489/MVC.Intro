using System.ComponentModel.DataAnnotations;

namespace MVC.Intro.Models
{
    public class LoginViewModel
    {
        [Display(Name = "Имейл")]
        [Required(ErrorMessage = "Въведете имейл")]
        [EmailAddress(ErrorMessage = "Невалиден имейл")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Парола")]
        [Required(ErrorMessage = "Въведете парола")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Запомни ме")]
        public bool RememberMe { get; set; }
    }
}
