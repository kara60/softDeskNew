using System.ComponentModel.DataAnnotations;

namespace TicketSystem.API.Models
{
    public class FormField
    {
        public Guid Id { get; set; }

        [Required]
        public Guid TicketTypeId { get; set; }
        public TicketType TicketType { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string FieldName { get; set; } = string.Empty; // error_description, expected_behavior

        [Required]
        [MaxLength(100)]
        public string DisplayName { get; set; } = string.Empty; // Hata Açıklaması, Beklenen Davranış

        [Required]
        [MaxLength(50)]
        public string FieldType { get; set; } = string.Empty; // text, textarea, select, checkbox, file

        public string? DefaultValue { get; set; } // Varsayılan değer

        public string? PlaceholderText { get; set; } // Placeholder metni

        public string? HelpText { get; set; } // Yardım metni

        public bool IsRequired { get; set; } = false; // Zorunlu alan mı?

        public int SortOrder { get; set; } = 0; // Form içinde sıralama

        public int? MinLength { get; set; } // Minimum karakter
        public int? MaxLength { get; set; } // Maksimum karakter

        public string? ValidationRules { get; set; } // JSON: {"pattern": "email", "min": 5}

        // Select, radio, checkbox için seçenekler (JSON)
        public string? Options { get; set; } // [{"value":"option1","text":"Seçenek 1"}]

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<TicketFormData> FormData { get; set; } = new List<TicketFormData>();
    }
}