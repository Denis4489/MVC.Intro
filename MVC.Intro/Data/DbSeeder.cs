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
                    Description = "Елесно дерби на Етихад"
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
        }
    }
}
