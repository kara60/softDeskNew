using System.ComponentModel.DataAnnotations;
using System.Net.Sockets;

namespace TicketSystem.API.Models
{
    public class Ticket
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string TicketNumber { get; set; } = string.Empty; // TK-2024-001

        [Required]
        public Guid CompanyId { get; set; }
        public Company Company { get; set; } = null!;

        [Required]
        public Guid CreatedByUserId { get; set; }
        public User CreatedByUser { get; set; } = null!;

        public Guid? AssignedToUserId { get; set; }
        public User? AssignedToUser { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        // Ticket türü (form belirler)
        [Required]
        public Guid TicketTypeId { get; set; }
        public TicketType TicketType { get; set; } = null!;

        // Kategori (modül listesi belirler)
        public Guid? CategoryId { get; set; }
        public TicketCategory? Category { get; set; }

        // Seçilen modül/ekran
        public Guid? ModuleId { get; set; }
        public TicketModule? Module { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Open"; // Open, InProgress, Resolved, Closed

        [MaxLength(10)]
        public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical

        // İş emri dönüşümü için
        public bool IsConvertedToWorkOrder { get; set; } = false;
        public string? WorkOrderId { get; set; }
        public DateTime? ConvertedToWorkOrderAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
        public ICollection<TicketAttachment> Attachments { get; set; } = new List<TicketAttachment>();
        public ICollection<TicketFormData> FormData { get; set; } = new List<TicketFormData>();
    }
}