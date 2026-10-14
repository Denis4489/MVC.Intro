namespace MVC.Intro.Models
{
    public class CreateEventViewModel
    {
        public Event Event { get; set; } = new() { Name = "", Venue = "", TicketType = "" };
        public List<string> AvailableImages { get; set; } = new();
    }
}
