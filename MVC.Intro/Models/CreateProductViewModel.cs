namespace MVC.Intro.Models
{
    public class CreateProductViewModel
    {
        public Product Product { get; set; } = new() { Name = "" };
        public List<string> AvailableImages { get; set; } = new();
    }
}
