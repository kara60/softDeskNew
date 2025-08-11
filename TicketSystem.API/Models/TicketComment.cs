using System.ComponentModel.DataAnnotations;

namespace TicketSystem.API.Models
{
    public class TicketComment
    {
        public Guid Id { get; set; }

        [Required]
        public Guid TicketId { get; set; }
        public Ticket Ticket { get; set; } = null!;

        [Required]
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        [Required]
        public string Comment { get; set; } = string.Empty;

        public bool IsInternal { get; set; } = false; // Sadece support ekibi görsün

        public bool IsSystemMessage { get; set; } = false; // Sistem mesajı (durum değişikliği vs.)

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}