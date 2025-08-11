using System.ComponentModel.DataAnnotations;

namespace TicketSystem.API.Models
{
    public class SystemSettings
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string SettingKey { get; set; } = string.Empty; // PMO_INTEGRATION_ENABLED

        [Required]
        public string SettingValue { get; set; } = string.Empty; // true, false, url, json

        [Required]
        [MaxLength(50)]
        public string DataType { get; set; } = "string"; // string, boolean, number, json

        [MaxLength(200)]
        public string? DisplayName { get; set; } // PMO Entegrasyonu Aktif

        [MaxLength(500)]
        public string? Description { get; set; } // Ticket onaylandığında otomatik olarak PMO'ya iş oluşturulsun

        [MaxLength(50)]
        public string? Category { get; set; } = "General"; // General, PMO, Email, Security

        public bool IsSystemSetting { get; set; } = false; // Kullanıcı değiştirebilir mi?

        public bool IsVisible { get; set; } = true; // Admin panelinde gösterilsin mi?

        public string? DefaultValue { get; set; } // Varsayılan değer

        public string? ValidationRules { get; set; } // JSON: {"required": true, "pattern": "url"}

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}