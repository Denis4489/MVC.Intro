namespace MVC.Intro.Models
{
    public class ShopProductViewModel
    {
        public Product Product { get; set; } = null!;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Accent { get; set; } = "navy";
    }
}
