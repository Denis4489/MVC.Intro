using System.ComponentModel.DataAnnotations;

namespace MVC.Intro.Models
{
    public class CreateEventViewModel : IValidatableObject
    {
        public Event Event { get; set; } = new() { Name = "", Venue = "", TicketType = "" };
        public List<string> AvailableImages { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Event != null && Event.AvailableTickets > Event.Capacity)
            {
                yield return new ValidationResult(
                    "Наличните билети не могат да надвишават капацитета.",
                    new[] { "Event.AvailableTickets" });
            }
            if (Event != null && Event.EventDate <= DateTime.Now)
            {
                yield return new ValidationResult(
                    "Датата на събитието трябва да е в бъдещето.",
                    new[] { "Event.EventDate" });
            }
        }
    }
}
