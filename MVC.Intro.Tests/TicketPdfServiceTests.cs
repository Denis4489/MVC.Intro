using System.Text;
using MVC.Intro.Models;
using MVC.Intro.Services;

namespace MVC.Intro.Tests
{
    public class TicketPdfServiceTests
    {
        private readonly TicketPdfService _service = new();

        [Fact]
        public void GenerateQrCodePng_ShouldReturnValidPngBytes()
        {
            var bytes = _service.GenerateQrCodePng("Ticket-Test-Code");

            Assert.NotNull(bytes);
            Assert.NotEmpty(bytes);
            // PNG signature header: 0x89, 0x50, 0x4E, 0x47 (PNG)
            Assert.Equal(0x89, bytes[0]);
            Assert.Equal(0x50, bytes[1]);
            Assert.Equal(0x4E, bytes[2]);
            Assert.Equal(0x47, bytes[3]);
        }

        [Fact]
        public void GenerateQrCodeBase64_ShouldReturnValidDataUri()
        {
            var base64 = _service.GenerateQrCodeBase64("Ticket-Test-Code");

            Assert.NotNull(base64);
            Assert.StartsWith("data:image/png;base64,", base64);
        }

        [Fact]
        public void GenerateTicketPdf_WithMultipleTickets_ShouldGenerateValidPdf()
        {
            var qr1 = _service.GenerateQrCodeBase64("PAYLOAD-1");
            var qr2 = _service.GenerateQrCodeBase64("PAYLOAD-2");

            var order = new OrderConfirmationViewModel
            {
                OrderId = "TKT-20261023-TEST01",
                OrderDate = DateTime.Now,
                CustomerName = "Иван Петров",
                CustomerEmail = "ivan@test.com",
                TotalAmount = 240.00m,
                TotalItems = 3,
                Tickets = new List<TicketItemViewModel>
                {
                    new()
                    {
                        EventId = Guid.NewGuid(),
                        EventName = "Manchester City vs Liverpool",
                        Category = "Премиър Лига",
                        Venue = "Etihad Stadium",
                        EventDate = DateTime.Now.AddDays(10),
                        TicketType = "VIP",
                        Quantity = 2,
                        UnitPrice = 100.00m,
                        TicketCode = "TKT-20261023-TEST01-01",
                        QrPayload = "PAYLOAD-1",
                        QrCodeBase64 = qr1
                    },
                    new()
                    {
                        EventId = Guid.NewGuid(),
                        EventName = "Real Madrid vs Barcelona",
                        Category = "Ла Лига",
                        Venue = "Santiago Bernabeu",
                        EventDate = DateTime.Now.AddDays(14),
                        TicketType = "Стандартен",
                        Quantity = 1,
                        UnitPrice = 40.00m,
                        TicketCode = "TKT-20261023-TEST01-02",
                        QrPayload = "PAYLOAD-2",
                        QrCodeBase64 = qr2
                    }
                }
            };

            var pdfBytes = _service.GenerateTicketPdf(order);

            Assert.NotNull(pdfBytes);
            Assert.NotEmpty(pdfBytes);

            // PDF header signature: %PDF-
            var header = Encoding.ASCII.GetString(pdfBytes.AsSpan(0, 5));
            Assert.Equal("%PDF-", header);
        }

        [Fact]
        public void GenerateTicketPdf_ShouldSaveValidPdfFile()
        {
            var qr = _service.GenerateQrCodeBase64("TEST-SAMPLE-TICKET");

            var order = new OrderConfirmationViewModel
            {
                OrderId = "TKT-SAMPLE-99",
                OrderDate = DateTime.Now,
                CustomerName = "Тестов Потребител",
                CustomerEmail = "test@example.com",
                TotalAmount = 55.00m,
                TotalItems = 1,
                Tickets = new List<TicketItemViewModel>
                {
                    new()
                    {
                        EventId = Guid.NewGuid(),
                        EventName = "Juventus vs AC Milan",
                        Category = "Серия А",
                        Venue = "Allianz Stadium",
                        EventDate = DateTime.Now.AddDays(5),
                        TicketType = "Трибуна Изток",
                        Quantity = 1,
                        UnitPrice = 55.00m,
                        TicketCode = "TKT-SAMPLE-99-01",
                        QrPayload = "TEST-SAMPLE-TICKET",
                        QrCodeBase64 = qr
                    }
                }
            };

            var pdfBytes = _service.GenerateTicketPdf(order);
            Assert.NotNull(pdfBytes);
            Assert.True(pdfBytes.Length > 1000, "PDF size should be at least 1KB");

            var tempPath = Path.Combine(Path.GetTempPath(), "test_ticket.pdf");
            File.WriteAllBytes(tempPath, pdfBytes);
            Assert.True(File.Exists(tempPath));
            File.Delete(tempPath);
        }
    }
}
