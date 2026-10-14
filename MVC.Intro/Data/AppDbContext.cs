using Microsoft.EntityFrameworkCore;
using MVC.Intro.Models;

namespace MVC.Intro.Data
{
    public class AppDbContext : DbContext
    {
        public string DbPath { get; }

        public AppDbContext() : base(new DbContextOptions<AppDbContext>())
        {
            var basePath = Directory.GetCurrentDirectory();
            var dataPath = Path.Combine(basePath, "Data");
            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }
            DbPath = Path.Combine(dataPath, "events.db");
        }

        public DbSet<Event> Events { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (!options.IsConfigured)
            {
                options.UseSqlite($"Data Source={DbPath}");
            }
        }

    }
}
