namespace MVC.Intro.Services
{
    public static class EventCatalog
    {
        public static readonly IReadOnlyDictionary<string, CatalogEntry> ByName =
            new Dictionary<string, CatalogEntry>(StringComparer.OrdinalIgnoreCase)
            {
                ["Manchester City vs Liverpool"] = new("Премиър Лига", "Елесно дерби на Етихад.", "blue"),
                ["Real Madrid vs Barcelona"] = new("Ла Лига", "Ел Класико на Сантиаго Бернабеу.", "white"),
                ["Bayern Munich vs Borussia Dortmund"] = new("Бундеслига", "Дерби на Германия на Алианц Арена.", "red"),
                ["Juventus vs AC Milan"] = new("Серия А", "Италианското дерби.", "black"),
                ["PSG vs Marseille"] = new("Лига 1", "Класически дерби на Парк де Пренс.", "navy"),
                ["Inter vs AC Milan"] = new("Серия А", "Миланското дерби.", "blue"),
                ["Chelsea vs Arsenal"] = new("Премиър Лига", "Лондонското дерби.", "blue"),
                ["Atletico Madrid vs Real Madrid"] = new("Ла Лига", "Мадридското дерби.", "red"),
                ["Borussia Dortmund vs Schalke"] = new("Бундеслига", "Рурското дерби.", "yellow")
            };

        public static CatalogEntry Get(string name) =>
            ByName.TryGetValue(name, out var entry)
                ? entry
                : new("Спорт", "Спортно събитие.", "gray");

        public record CatalogEntry(string Category, string Description, string Accent);
    }
}
