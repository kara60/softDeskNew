using System.ComponentModel.DataAnnotations;

namespace TicketSystem.API.Models
{
    public class TicketModule
    {
        public Guid Id { get; set; }

        [Required]
        public Guid CategoryId { get; set; }
        public TicketCategory Category { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty; // Muhasebe Modülü, Stok Yönetimi, Satış Süreci

        [MaxLength(500)]
        public string? Description { get; set; } // Fatura, cari hesap, mali raporlama işlemleri

        [MaxLength(50)]
        public string? Icon { get; set; } // 💰 📦 📈

        [MaxLength(20)]
        public string? Color { get; set; } // #10b981

        public int SortOrder { get; set; } = 0; // Kategori içinde sıralama

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}