using System.ComponentModel.DataAnnotations;

namespace TicketSystem.API.Models
{
    public class TicketFormData
    {
        public Guid Id { get; set; }

        [Required]
        public Guid TicketId { get; set; }
        public Ticket Ticket { get; set; } = null!;

        [Required]
        public Guid FormFieldId { get; set; }
        public FormField FormField { get; set; } = null!;

        [Required]
        public string FieldValue { get; set; } = string.Empty; // Kullanıcının girdiği değer

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}