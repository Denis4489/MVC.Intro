using Microsoft.EntityFrameworkCore;
using MVC.Intro.Models;

namespace MVC.Intro.Data
{
    public static class DbSeeder
    {
        public static void Seed(AppDbContext db)
        {
            db.Database.EnsureCreated();

            var events = new[]
            {
                // Premier League
                new Event
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Name = "Manchester City vs Liverpool",
                    Price = 75.00m,
                    ImageUrl = "/images/PSGNIKEKIT.jpg",
                    EventDate = DateTime.Parse("2025-02-15 17:30"),
                    Venue = "Etihad Stadium",
                    TicketType = "Standard",
                    Capacity = 53000,
                    AvailableTickets = 48000,
                    Description = "Дерби на Етихад"
                },
                new Event
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Name = "Chelsea vs Arsenal",
                    Price = 65.00m,
                    ImageUrl = "/images/DembeleKit.jpg",
                    EventDate = DateTime.Parse("2025-03-01 16:00"),
                    Venue = "Stamford Bridge",
                    TicketType = "Standard",
                    Capacity = 40000,
                    AvailableTickets = 35000,
                    Description = "Лондонското дерби"
                },
                // La Liga
                new Event
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Name = "Real Madrid vs Barcelona",
                    Price = 120.00m,
                    ImageUrl = "/images/image.jpg",
                    EventDate = DateTime.Parse("2025-02-22 21:00"),
                    Venue = "Santiago Bernabeu",
                    TicketType = "VIP",
                    Capacity = 81000,
                    AvailableTickets = 65000,
                    Description = "Ел Класико"
                },
                new Event
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    Name = "Atletico Madrid vs Real Madrid",
                    Price = 55.00m,
                    ImageUrl = "/images/PSGNIKEKIT.jpg",
                    EventDate = DateTime.Parse("2025-04-05 18:30"),
                    Venue = "Wanda Metropolitano",
                    TicketType = "Standard",
                    Capacity = 68000,
                    AvailableTickets = 60000,
                    Description = "Мадридското дерби"
                },
                // Bundesliga
                new Event
                {
                    Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                    Name = "Bayern Munich vs Borussia Dortmund",
                    Price = 80.00m,
                    ImageUrl = "/images/DembeleKit.jpg",
                    EventDate = DateTime.Parse("2025-03-15 18:30"),
                    Venue = "Allianz Arena",
                    TicketType = "Standard",
                    Capacity = 75000,
                    AvailableTickets = 70000,
                    Description = "Дерби на Германия"
                },
                new Event
                {
                    Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                    Name = "Borussia Dortmund vs Schalke",
                    Price = 45.00m,
                    ImageUrl = "/images/image.jpg",
                    EventDate = DateTime.Parse("2025-02-28 15:30"),
                    Venue = "Signal Iduna Park",
                    TicketType = "Standard",
                    Capacity = 81000,
                    AvailableTickets = 75000,
                    Description = "Рурското дерби"
                },
                // Serie A
                new Event
                {
                    Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                    Name = "Juventus vs AC Milan",
                    Price = 60.00m,
                    ImageUrl = "/images/PSGNIKEKIT.jpg",
                    EventDate = DateTime.Parse("2025-03-22 20:45"),
                    Venue = "Allianz Stadium",
                    TicketType = "Standard",
                    Capacity = 41000,
                    AvailableTickets = 38000,
                    Description = "Италианското дерби"
                },
                new Event
                {
                    Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                    Name = "Inter vs AC Milan",
                    Price = 70.00m,
                    ImageUrl = "/images/DembeleKit.jpg",
                    EventDate = DateTime.Parse("2025-04-19 18:00"),
                    Venue = "San Siro",
                    TicketType = "Standard",
                    Capacity = 80000,
                    AvailableTickets = 72000,
                    Description = "Миланското дерби"
                },
                // Ligue 1
                new Event
                {
                    Id = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                    Name = "PSG vs Marseille",
                    Price = 50.00m,
                    ImageUrl = "/images/image.jpg",
                    EventDate = DateTime.Parse("2025-03-08 21:00"),
                    Venue = "Parc des Princes",
                    TicketType = "Standard",
                    Capacity = 48000,
                    AvailableTickets = 42000,
                    Description = "Класически дерби"
                }
            };

            foreach (var @event in events)
            {
                if (!db.Events.Any(existing => existing.Id == @event.Id || existing.Name == @event.Name))
                {
                    db.Events.Add(@event);
                }
            }

            db.SaveChanges();

            // Create Orders table if not exists and seed sample orders
            db.Database.ExecuteSqlRaw(@"
                CREATE TABLE IF NOT EXISTS ""Orders"" (
                    ""Id"" TEXT NOT NULL CONSTRAINT ""PK_Orders"" PRIMARY KEY,
                    ""OrderNumber"" TEXT NOT NULL,
                    ""OrderDate"" TEXT NOT NULL,
                    ""CustomerName"" TEXT NOT NULL,
                    ""CustomerEmail"" TEXT NULL,
                    ""TotalAmount"" TEXT NOT NULL,
                    ""TotalItems"" INTEGER NOT NULL,
                    ""TicketsSummary"" TEXT NULL
                );");

            if (!db.Orders.Any())
            {
                var today = DateTime.Today;
                db.Orders.AddRange(
                    new OrderRecord
                    {
                        Id = Guid.NewGuid(),
                        OrderNumber = "TKT-20261115-A101",
                        OrderDate = today.AddDays(-4).AddHours(14).AddMinutes(20),
                        CustomerName = "Димитър Бербатов",
                        CustomerEmail = "berbo@football.bg",
                        TotalAmount = 150.00m,
                        TotalItems = 2,
                        TicketsSummary = "Manchester City vs Liverpool (2 бр.)"
                    },
                    new OrderRecord
                    {
                        Id = Guid.NewGuid(),
                        OrderNumber = "TKT-20261116-B202",
                        OrderDate = today.AddDays(-3).AddHours(10).AddMinutes(15),
                        CustomerName = "Христо Стоичков",
                        CustomerEmail = "hristo@barca.bg",
                        TotalAmount = 240.00m,
                        TotalItems = 2,
                        TicketsSummary = "Real Madrid vs Barcelona (2 бр.)"
                    },
                    new OrderRecord
                    {
                        Id = Guid.NewGuid(),
                        OrderNumber = "TKT-20261117-C301",
                        OrderDate = today.AddDays(-2).AddHours(11).AddMinutes(5),
                        CustomerName = "Георги Иванов",
                        CustomerEmail = "georgi@fan.com",
                        TotalAmount = 130.00m,
                        TotalItems = 2,
                        TicketsSummary = "Chelsea vs Arsenal (2 бр.)"
                    },
                    new OrderRecord
                    {
                        Id = Guid.NewGuid(),
                        OrderNumber = "TKT-20261117-C302",
                        OrderDate = today.AddDays(-2).AddHours(13).AddMinutes(40),
                        CustomerName = "Мария Петрова",
                        CustomerEmail = "maria@test.bg",
                        TotalAmount = 80.00m,
                        TotalItems = 1,
                        TicketsSummary = "Bayern Munich vs Borussia Dortmund (1 бр.)"
                    },
                    new OrderRecord
                    {
                        Id = Guid.NewGuid(),
                        OrderNumber = "TKT-20261117-C303",
                        OrderDate = today.AddDays(-2).AddHours(16).AddMinutes(25),
                        CustomerName = "Николай Димитров",
                        CustomerEmail = "niki@fan.bg",
                        TotalAmount = 180.00m,
                        TotalItems = 3,
                        TicketsSummary = "Juventus vs AC Milan (3 бр.)"
                    },
                    new OrderRecord
                    {
                        Id = Guid.NewGuid(),
                        OrderNumber = "TKT-20261117-C304",
                        OrderDate = today.AddDays(-2).AddHours(18).AddMinutes(50),
                        CustomerName = "Стефан Попов",
                        CustomerEmail = "stefan@mail.bg",
                        TotalAmount = 120.00m,
                        TotalItems = 1,
                        TicketsSummary = "Real Madrid vs Barcelona (1 бр.)"
                    },
                    new OrderRecord
                    {
                        Id = Guid.NewGuid(),
                        OrderNumber = "TKT-20261118-D401",
                        OrderDate = today.AddDays(-1).AddHours(12).AddMinutes(10),
                        CustomerName = "Александър Тодоров",
                        CustomerEmail = "alex@fan.com",
                        TotalAmount = 90.00m,
                        TotalItems = 2,
                        TicketsSummary = "Borussia Dortmund vs Schalke (2 бр.)"
                    },
                    new OrderRecord
                    {
                        Id = Guid.NewGuid(),
                        OrderNumber = "TKT-20261118-D402",
                        OrderDate = today.AddDays(-1).AddHours(15).AddMinutes(35),
                        CustomerName = "Елена Василева",
                        CustomerEmail = "elena@tickets.bg",
                        TotalAmount = 110.00m,
                        TotalItems = 2,
                        TicketsSummary = "Atletico Madrid vs Real Madrid (2 бр.)"
                    },
                    new OrderRecord
                    {
                        Id = Guid.NewGuid(),
                        OrderNumber = "TKT-20261119-E501",
                        OrderDate = today.AddHours(9).AddMinutes(15),
                        CustomerName = "Петър Стоянов",
                        CustomerEmail = "petar@fan.com",
                        TotalAmount = 75.00m,
                        TotalItems = 1,
                        TicketsSummary = "Manchester City vs Liverpool (1 бр.)"
                    }
                );
                db.SaveChanges();
            }
        }
    }
}
