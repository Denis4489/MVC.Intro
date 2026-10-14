namespace MVC.Intro.Models
{
    public class EventViewModel
    {
        public Event Event { get; set; } = new() { Name = "", Venue = "", TicketType = "" };
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Accent { get; set; } = "navy";
    }
}
