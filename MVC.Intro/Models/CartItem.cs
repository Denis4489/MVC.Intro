namespace MVC.Intro.Models
{
    public class CartItem
    {
        public Guid ProductId { get; set; }
        public string Sector { get; set; } = "Сектор 104 (VIP)";
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
