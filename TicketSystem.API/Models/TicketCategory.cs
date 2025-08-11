using System.ComponentModel.DataAnnotations;

namespace TicketSystem.API.Models
{
    public class TicketCategory
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty; // ERP Sistemi, CRM, Lopus

        [MaxLength(500)]
        public string? Description { get; set; } // Muhasebe, Stok, Satış işlemleri

        [MaxLength(50)]
        public string? Icon { get; set; } // 📊 💼 👥

        [MaxLength(20)]
        public string? Color { get; set; } // #f59e0b

        public int SortOrder { get; set; } = 0; // Sıralama için

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
        public ICollection<TicketModule> Modules { get; set; } = new List<TicketModule>(); // Bu kategoriye ait modüller
    }
}