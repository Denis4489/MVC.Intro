using System.Text.Json.Serialization;

namespace MVC.Intro.Models
{
    public class OrderConfirmationViewModel
    {
        public string OrderId { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int TotalItems { get; set; }
        public List<TicketItemViewModel> Tickets { get; set; } = new();

        public string PrimaryQrCodeBase64 => Tickets.FirstOrDefault()?.QrCodeBase64 ?? string.Empty;
    }

    public class TicketItemViewModel
    {
        public Guid EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Venue { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string TicketType { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;
        public string TicketCode { get; set; } = string.Empty;
        public string QrPayload { get; set; } = string.Empty;
        public string QrCodeBase64 { get; set; } = string.Empty;

        [JsonIgnore]
        public byte[] QrCodeBytes
        {
            get
            {
                if (string.IsNullOrEmpty(QrCodeBase64))
                {
                    return Array.Empty<byte>();
                }

                var clean = QrCodeBase64;
                var prefixIndex = clean.IndexOf("base64,", StringComparison.OrdinalIgnoreCase);
                if (prefixIndex >= 0)
                {
                    clean = clean[(prefixIndex + 7)..];
                }

                try
                {
                    return Convert.FromBase64String(clean);
                }
                catch
                {
                    return Array.Empty<byte>();
                }
            }
        }
    }
}
