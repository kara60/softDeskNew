using System.ComponentModel.DataAnnotations;

namespace TicketSystem.API.Models
{
    public class TicketAttachment
    {
        public Guid Id { get; set; }

        [Required]
        public Guid TicketId { get; set; }
        public Ticket Ticket { get; set; } = null!;

        [Required]
        public Guid UploadedByUserId { get; set; }
        public User UploadedByUser { get; set; } = null!;

        [Required]
        [MaxLength(255)]
        public string FileName { get; set; } = string.Empty; // error-screenshot.png

        [Required]
        [MaxLength(255)]
        public string FilePath { get; set; } = string.Empty; // /uploads/tickets/2024/01/guid.png

        [Required]
        [MaxLength(100)]
        public string ContentType { get; set; } = string.Empty; // image/png, application/pdf

        public long FileSize { get; set; } // Byte cinsinden dosya boyutu

        [MaxLength(500)]
        public string? Description { get; set; } // Dosya açıklaması

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}