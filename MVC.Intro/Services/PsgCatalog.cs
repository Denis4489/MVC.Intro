namespace MVC.Intro.Services
{
    public static class PsgCatalog
    {
        public static readonly IReadOnlyDictionary<string, CatalogEntry> ByName =
            new Dictionary<string, CatalogEntry>(StringComparer.OrdinalIgnoreCase)
            {
                ["Домашна фланелка"] = new("Екипи", "Официалният домакински екип в тъмносиньо и червено.", "navy"),
                ["Гостуваща фланелка"] = new("Екипи", "Гостуващ екип в бяло с клубните акценти.", "white"),
                ["Тренировъчен топ"] = new("Екипи", "Лек тренировъчен топ за мачове и ежедневие.", "red"),
                ["Клубна шалче"] = new("Аксесоари", "Двулицево шалче за трибуните на Парк де Пренс.", "navy"),
                ["Топка за мач"] = new("Екипировка", "Официална топка за тренировки и мачове.", "white"),
                ["Кепе на ПСЖ"] = new("Аксесоари", "Класическо кепе с клубните цветове.", "red"),
                ["Детски екип"] = new("Деца", "Домакински екип за малките фенове.", "navy"),
                ["Раница"] = new("Аксесоари", "Спортна раница за стадиона и пътуване.", "red")
            };

        public static CatalogEntry Get(string name) =>
            ByName.TryGetValue(name, out var entry)
                ? entry
                : new("Магазин", "Официален артикул на Paris Saint-Germain.", "navy");

        public record CatalogEntry(string Category, string Description, string Accent);
    }
}
