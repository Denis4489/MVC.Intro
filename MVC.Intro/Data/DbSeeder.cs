using MVC.Intro.Models;

namespace MVC.Intro.Data
{
    public static class DbSeeder
    {
        public static void Seed(AppDbContext db)
        {
            db.Database.EnsureCreated();

            var catalog = new[]
            {
                new Product { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Домашна фланелка", Price = 129.99m, ImageUrl = "/images/PSGNIKEKIT.jpg" },
                new Product { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Гостуваща фланелка", Price = 119.99m, ImageUrl = "/images/PSGNIKEKIT.jpg" },
                new Product { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Тренировъчен топ", Price = 79.99m, ImageUrl = "/images/PSGNIKEKIT.jpg" },
                new Product { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), Name = "Клубна шалче", Price = 29.99m, ImageUrl = "/images/PSGNIKEKIT.jpg" },
                new Product { Id = Guid.Parse("55555555-5555-5555-5555-555555555555"), Name = "Топка за мач", Price = 39.99m, ImageUrl = "/images/PSGNIKEKIT.jpg" },
                new Product { Id = Guid.Parse("66666666-6666-6666-6666-666666666666"), Name = "Кепе на ПСЖ", Price = 27.99m, ImageUrl = "/images/PSGNIKEKIT.jpg" },
                new Product { Id = Guid.Parse("77777777-7777-7777-7777-777777777777"), Name = "Детски екип", Price = 69.99m, ImageUrl = "/images/PSGNIKEKIT.jpg" },
                new Product { Id = Guid.Parse("88888888-8888-8888-8888-888888888888"), Name = "Раница", Price = 54.99m, ImageUrl = "/images/PSGNIKEKIT.jpg" }
            };

            foreach (var product in catalog)
            {
                if (!db.Products.Any(existing => existing.Id == product.Id || existing.Name == product.Name))
                {
                    db.Products.Add(product);
                }
            }

            db.SaveChanges();
        }
    }
}
