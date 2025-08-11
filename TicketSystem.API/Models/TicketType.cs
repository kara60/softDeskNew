using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace TicketSystem.API.Models
{
    public class TicketType
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty; // Hata/Sorun Bildirimi

        [MaxLength(500)]
        public string? Description { get; set; } // Sistem hataları ve teknik sorunlar için acil destek

        [MaxLength(50)]
        public string Icon { get; set; } = "📋"; // Emoji veya CSS class

        [MaxLength(20)]
        public string Color { get; set; } = "#6366f1"; // Hex color code

        public int SortOrder { get; set; } = 0; // Sıralama için

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
        public ICollection<FormField> FormFields { get; set; } = new List<FormField>(); // Bu ticket türü için form alanları
    }
}