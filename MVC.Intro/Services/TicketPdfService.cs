using MVC.Intro.Models;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MVC.Intro.Services
{
    public class TicketPdfService
    {
        static TicketPdfService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
            QuestPDF.Settings.UseSystemFonts = true;
            QuestPDF.Settings.ThrowOnMissingFontFamilies = false;
        }

        public byte[] GenerateQrCodePng(string payload)
        {
            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
            var qr = new PngByteQRCode(data);
            return qr.GetGraphic(20);
        }

        public string GenerateQrCodeBase64(string payload)
        {
            var bytes = GenerateQrCodePng(payload);
            return $"data:image/png;base64,{Convert.ToBase64String(bytes)}";
        }

        public byte[] GenerateTicketPdf(OrderConfirmationViewModel order)
        {
            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(24);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10).FontColor(Colors.Grey.Darken4));

                    page.Header().Element(c => ComposeHeader(c, order));
                    page.Content().Element(c => ComposeContent(c, order));
                    page.Footer().Element(ComposeFooter);
                });
            });

            return doc.GeneratePdf();
        }

        private static void ComposeHeader(IContainer container, OrderConfirmationViewModel order)
        {
            container.PaddingBottom(16).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("🎫 TICKETS ARENA").FontSize(22).Bold().FontColor(Color.FromHex("#1e40af"));
                    col.Item().Text("Официален електронен билет за достъп").FontSize(10).FontColor(Colors.Grey.Darken1);
                });

                row.RelativeItem().AlignRight().Column(col =>
                {
                    col.Item().Text($"Поръчка № {order.OrderId}").FontSize(12).Bold().FontColor(Color.FromHex("#0f172a"));
                    col.Item().Text($"Дата: {order.OrderDate:dd.MM.yyyy HH:mm}").FontSize(9).FontColor(Colors.Grey.Darken1);
                    col.Item().Text($"Притежател: {order.CustomerName}").FontSize(9).FontColor(Colors.Grey.Darken2);
                });
            });
        }

        private static void ComposeContent(IContainer container, OrderConfirmationViewModel order)
        {
            container.Column(col =>
            {
                col.Spacing(14);

                foreach (var ticket in order.Tickets)
                {
                    col.Item().Border(1).BorderColor(Color.FromHex("#cbd5e1")).CornerRadius(10).Background(Color.FromHex("#f8fafc")).Padding(12).Row(row =>
                    {
                        // Ticket Details (Left Side)
                        row.RelativeItem(7).Column(tCol =>
                        {
                            tCol.Item().Row(r =>
                            {
                                r.AutoItem().Background(Color.FromHex("#2563eb")).PaddingHorizontal(8).PaddingVertical(3).CornerRadius(4)
                                    .Text(ticket.Category).FontSize(8).Bold().FontColor(Colors.White);

                                if (!string.IsNullOrWhiteSpace(ticket.TicketType))
                                {
                                    r.AutoItem().PaddingLeft(6).Background(Color.FromHex("#f59e0b")).PaddingHorizontal(8).PaddingVertical(3).CornerRadius(4)
                                        .Text(ticket.TicketType).FontSize(8).Bold().FontColor(Color.FromHex("#0f172a"));
                                }
                            });

                            tCol.Item().PaddingTop(6).Text(ticket.EventName).FontSize(16).Bold().FontColor(Color.FromHex("#0f172a"));

                            tCol.Item().PaddingTop(6).Row(infoRow =>
                            {
                                infoRow.RelativeItem().Column(c1 =>
                                {
                                    c1.Spacing(3);
                                    c1.Item().Text(t =>
                                    {
                                        t.Span("📍 Място: ").Bold().FontColor(Colors.Grey.Darken3);
                                        t.Span(string.IsNullOrWhiteSpace(ticket.Venue) ? "Централна Арена" : ticket.Venue);
                                    });
                                    c1.Item().Text(t =>
                                    {
                                        t.Span("📅 Дата: ").Bold().FontColor(Colors.Grey.Darken3);
                                        t.Span(ticket.EventDate != default ? ticket.EventDate.ToString("dd.MM.yyyy HH:mm") : "По график");
                                    });
                                });

                                infoRow.RelativeItem().Column(c2 =>
                                {
                                    c2.Spacing(3);
                                    c2.Item().Text(t =>
                                    {
                                        t.Span("👥 Количество: ").Bold().FontColor(Colors.Grey.Darken3);
                                        t.Span($"{ticket.Quantity} бр.");
                                    });
                                    c2.Item().Text(t =>
                                    {
                                        t.Span("💰 Ед. цена: ").Bold().FontColor(Colors.Grey.Darken3);
                                        t.Span($"{ticket.UnitPrice:0.00} €");
                                    });
                                });
                            });

                            tCol.Item().PaddingTop(8).BorderTop(1).BorderColor(Color.FromHex("#e2e8f0")).PaddingTop(6).Row(subRow =>
                            {
                                subRow.RelativeItem().Text($"Код за валидация: {ticket.TicketCode}").FontSize(8).FontColor(Colors.Grey.Darken1);
                                subRow.RelativeItem().AlignRight().Text($"Обща сума: {ticket.TotalPrice:0.00} €").FontSize(11).Bold().FontColor(Color.FromHex("#1e40af"));
                            });
                        });

                        // Perforated line separator
                        row.ConstantItem(18).AlignCenter().Column(lineCol =>
                        {
                            lineCol.Item().Height(110).LineVertical(1).LineColor(Color.FromHex("#94a3b8"));
                        });

                        // QR Code Side (Right Side)
                        row.RelativeItem(3).Column(qrCol =>
                        {
                            qrCol.Item().AlignCenter().Text("СКАНИРАЙ ЗА ВХОД").FontSize(8).Bold().FontColor(Color.FromHex("#1e40af"));

                            var qrBytes = ticket.QrCodeBytes;
                            if (qrBytes.Length > 0)
                            {
                                qrCol.Item().PaddingTop(4).AlignCenter().Width(95).Height(95).Image(qrBytes);
                            }

                            qrCol.Item().PaddingTop(4).AlignCenter().Text(ticket.TicketCode).FontSize(8).FontFamily("Consolas").FontColor(Colors.Grey.Darken3);
                        });
                    });
                }

                // Summary Row
                col.Item().PaddingTop(8).BorderTop(1).BorderColor(Color.FromHex("#cbd5e1")).PaddingTop(8).Row(sumRow =>
                {
                    sumRow.RelativeItem().Text($"Общо билети: {order.TotalItems} бр.").FontSize(11).Bold().FontColor(Colors.Grey.Darken3);
                    sumRow.RelativeItem().AlignRight().Text($"Обща стойност: {order.TotalAmount:0.00} €").FontSize(14).Bold().FontColor(Color.FromHex("#1e40af"));
                });
            });
        }

        private static void ComposeFooter(IContainer container)
        {
            container.BorderTop(1).BorderColor(Color.FromHex("#e2e8f0")).PaddingTop(8).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("⚠️ Важно: Този билет е личен и служи за еднократен достъп. Всеки опит за препродажба или повторно сканиране е забранен.").FontSize(7.5f).FontColor(Colors.Grey.Darken1);
                    col.Item().Text("При проблеми на входа, моля обърнете се към персонала на събитието или сканирайте билета отново.").FontSize(7.5f).FontColor(Colors.Grey.Darken1);
                });

                row.RelativeItem().AlignRight().Column(col =>
                {
                    col.Item().Text("TICKETS ARENA | Система за билети").FontSize(8).Bold().FontColor(Colors.Grey.Darken2);
                    col.Item().Text("www.tickets.local").FontSize(7.5f).FontColor(Color.FromHex("#2563eb"));
                });
            });
        }
    }
}
