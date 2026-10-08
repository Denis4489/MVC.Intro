namespace MVC.Intro.Models
{
    public class CartLineViewModel
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal => Price * Quantity;
    }
}
