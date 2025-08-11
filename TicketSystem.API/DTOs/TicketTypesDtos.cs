using System.ComponentModel.DataAnnotations;

namespace TicketSystem.API.DTOs
{
    #region TicketType DTOs

    // Ticket türü listesi için
    public class TicketTypeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Icon { get; set; } = string.Empty; // 🐛, 💡, 🎓
        public string Color { get; set; } = string.Empty; // #ef4444
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public int FormFieldCount { get; set; } // Bu türe ait form alanı sayısı
    }

    // Ticket türü detayı için (form alanları dahil)
    public class TicketTypeDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public List<FormFieldDto> FormFields { get; set; } = new List<FormFieldDto>();
    }

    // Yeni ticket türü oluşturma için
    public class CreateTicketTypeDto
    {
        [Required(ErrorMessage = "Ticket türü adı gereklidir.")]
        [MaxLength(100, ErrorMessage = "Ticket türü adı en fazla 100 karakter olabilir.")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        public string? Description { get; set; }

        [MaxLength(50, ErrorMessage = "İkon en fazla 50 karakter olabilir.")]
        public string Icon { get; set; } = "📋"; // Default icon

        [MaxLength(20, ErrorMessage = "Renk kodu en fazla 20 karakter olabilir.")]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Geçerli bir hex renk kodu giriniz. (örn: #ff0000)")]
        public string Color { get; set; } = "#6366f1"; // Default color

        [Range(0, 1000, ErrorMessage = "Sıralama 0-1000 arasında olmalıdır.")]
        public int SortOrder { get; set; } = 0;
    }

    // Ticket türü güncelleme için
    public class UpdateTicketTypeDto
    {
        [Required(ErrorMessage = "Ticket türü adı gereklidir.")]
        [MaxLength(100, ErrorMessage = "Ticket türü adı en fazla 100 karakter olabilir.")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        public string? Description { get; set; }

        [MaxLength(50, ErrorMessage = "İkon en fazla 50 karakter olabilir.")]
        public string Icon { get; set; } = string.Empty;

        [MaxLength(20, ErrorMessage = "Renk kodu en fazla 20 karakter olabilir.")]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Geçerli bir hex renk kodu giriniz.")]
        public string Color { get; set; } = string.Empty;

        [Range(0, 1000, ErrorMessage = "Sıralama 0-1000 arasında olmalıdır.")]
        public int SortOrder { get; set; }

        public bool IsActive { get; set; } = true;
    }

    #endregion

    #region Category & Module DTOs

    // Kategori bilgisi
    public class TicketCategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty; // ERP Sistemi, CRM
        public string? Description { get; set; }
        public string? Icon { get; set; } // 📊, 👥
        public string? Color { get; set; } // #f59e0b
        public int SortOrder { get; set; }
        public int ModuleCount { get; set; } // Bu kategoriye ait modül sayısı
    }

    // Modül bilgisi
    public class TicketModuleDto
    {
        public Guid Id { get; set; }
        public Guid CategoryId { get; set; }
        public string Name { get; set; } = string.Empty; // Muhasebe, Stok Yönetimi
        public string? Description { get; set; }
        public string? Icon { get; set; } // 💰, 📦
        public string? Color { get; set; }
        public int SortOrder { get; set; }
    }

    // Yeni kategori oluşturma
    public class CreateCategoryDto
    {
        [Required(ErrorMessage = "Kategori adı gereklidir.")]
        [MaxLength(100, ErrorMessage = "Kategori adı en fazla 100 karakter olabilir.")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        public string? Description { get; set; }

        [MaxLength(50, ErrorMessage = "İkon en fazla 50 karakter olabilir.")]
        public string? Icon { get; set; }

        [MaxLength(20, ErrorMessage = "Renk kodu en fazla 20 karakter olabilir.")]
        public string? Color { get; set; }

        [Range(0, 1000, ErrorMessage = "Sıralama 0-1000 arasında olmalıdır.")]
        public int SortOrder { get; set; } = 0;
    }

    // Yeni modül oluşturma
    public class CreateModuleDto
    {
        [Required(ErrorMessage = "Modül adı gereklidir.")]
        [MaxLength(100, ErrorMessage = "Modül adı en fazla 100 karakter olabilir.")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        public string? Description { get; set; }

        [MaxLength(50, ErrorMessage = "İkon en fazla 50 karakter olabilir.")]
        public string? Icon { get; set; }

        [MaxLength(20, ErrorMessage = "Renk kodu en fazla 20 karakter olabilir.")]
        public string? Color { get; set; }

        [Range(0, 1000, ErrorMessage = "Sıralama 0-1000 arasında olmalıdır.")]
        public int SortOrder { get; set; } = 0;
    }

    #endregion

    #region Form Field DTOs

    // Form alanı bilgisi
    public class FormFieldDto
    {
        public Guid Id { get; set; }
        public string FieldName { get; set; } = string.Empty; // error_description
        public string DisplayName { get; set; } = string.Empty; // Hata Açıklaması
        public string FieldType { get; set; } = string.Empty; // text, textarea, select
        public string? DefaultValue { get; set; }
        public string? PlaceholderText { get; set; }
        public string? HelpText { get; set; }
        public bool IsRequired { get; set; }
        public int SortOrder { get; set; }
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
        public string? ValidationRules { get; set; } // JSON format
        public string? Options { get; set; } // JSON format for select/radio
    }

    // Yeni form alanı oluşturma
    public class CreateFormFieldDto
    {
        [Required(ErrorMessage = "Alan adı gereklidir.")]
        [MaxLength(100, ErrorMessage = "Alan adı en fazla 100 karakter olabilir.")]
        [RegularExpression(@"^[a-zA-Z][a-zA-Z0-9_]*$", ErrorMessage = "Alan adı harf ile başlamalı ve sadece harf, rakam, alt çizgi içerebilir.")]
        public string FieldName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Görünen ad gereklidir.")]
        [MaxLength(100, ErrorMessage = "Görünen ad en fazla 100 karakter olabilir.")]
        public string DisplayName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Alan türü gereklidir.")]
        public string FieldType { get; set; } = string.Empty; // text, textarea, select, etc.

        public string? DefaultValue { get; set; }

        [MaxLength(200, ErrorMessage = "Placeholder en fazla 200 karakter olabilir.")]
        public string? PlaceholderText { get; set; }

        [MaxLength(500, ErrorMessage = "Yardım metni en fazla 500 karakter olabilir.")]
        public string? HelpText { get; set; }

        public bool IsRequired { get; set; } = false;

        [Range(0, 1000, ErrorMessage = "Sıralama 0-1000 arasında olmalıdır.")]
        public int SortOrder { get; set; } = 0;

        [Range(0, 10000, ErrorMessage = "Minimum uzunluk 0-10000 arasında olmalıdır.")]
        public int? MinLength { get; set; }

        [Range(1, 10000, ErrorMessage = "Maksimum uzunluk 1-10000 arasında olmalıdır.")]
        public int? MaxLength { get; set; }

        public string? ValidationRules { get; set; } // JSON: {"pattern": "email", "min": 5}

        public string? Options { get; set; } // JSON: [{"value":"opt1","text":"Seçenek 1"}]
    }

    // Form alanı güncelleme
    public class UpdateFormFieldDto
    {
        [Required(ErrorMessage = "Alan adı gereklidir.")]
        [MaxLength(100, ErrorMessage = "Alan adı en fazla 100 karakter olabilir.")]
        public string FieldName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Görünen ad gereklidir.")]
        [MaxLength(100, ErrorMessage = "Görünen ad en fazla 100 karakter olabilir.")]
        public string DisplayName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Alan türü gereklidir.")]
        public string FieldType { get; set; } = string.Empty;

        public string? DefaultValue { get; set; }
        public string? PlaceholderText { get; set; }
        public string? HelpText { get; set; }
        public bool IsRequired { get; set; }
        public int SortOrder { get; set; }
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
        public string? ValidationRules { get; set; }
        public string? Options { get; set; }
        public bool IsActive { get; set; } = true;
    }

    #endregion

    #region Predefined Templates

    // Hazır form şablonları
    public static class FormFieldTemplates
    {
        public static class BugReport
        {
            public static readonly List<CreateFormFieldDto> Fields = new()
            {
                new CreateFormFieldDto
                {
                    FieldName = "error_description",
                    DisplayName = "Hata Açıklaması",
                    FieldType = "textarea",
                    PlaceholderText = "Karşılaştığınız hatayı detaylı şekilde açıklayın...",
                    HelpText = "Hata ile ilgili tüm detayları belirtin",
                    IsRequired = true,
                    SortOrder = 1,
                    MinLength = 10,
                    MaxLength = 1000
                },
                new CreateFormFieldDto
                {
                    FieldName = "expected_behavior",
                    DisplayName = "Beklenen Davranış",
                    FieldType = "textarea",
                    PlaceholderText = "Sistemin nasıl davranması gerekiyordu?",
                    HelpText = "Normal şartlarda ne olması bekleniyordu?",
                    IsRequired = true,
                    SortOrder = 2,
                    MaxLength = 500
                },
                new CreateFormFieldDto
                {
                    FieldName = "steps_to_reproduce",
                    DisplayName = "Hatayı Tekrar Etme Adımları",
                    FieldType = "textarea",
                    PlaceholderText = "1. İlk adım\n2. İkinci adım\n3. Hata oluştu",
                    HelpText = "Hatayı tekrar etmek için yapılması gereken adımları sıralayın",
                    IsRequired = false,
                    SortOrder = 3,
                    MaxLength = 1000
                },
                new CreateFormFieldDto
                {
                    FieldName = "urgency_level",
                    DisplayName = "Aciliyet Seviyesi",
                    FieldType = "select",
                    Options = @"[
                        {""value"":""low"",""text"":""Düşük - Normal iş akışını etkilemiyor""},
                        {""value"":""medium"",""text"":""Orta - İş akışını kısmen etkiliyor""},
                        {""value"":""high"",""text"":""Yüksek - İş akışını önemli ölçüde etkiliyor""},
                        {""value"":""critical"",""text"":""Kritik - İş akışı tamamen durdu""}
                    ]",
                    IsRequired = true,
                    SortOrder = 4
                }
            };
        }

        public static class TrainingRequest
        {
            public static readonly List<CreateFormFieldDto> Fields = new()
            {
                new CreateFormFieldDto
                {
                    FieldName = "training_subject",
                    DisplayName = "Eğitim Konusu",
                    FieldType = "text",
                    PlaceholderText = "Hangi konuda eğitim almak istiyorsunuz?",
                    IsRequired = true,
                    SortOrder = 1,
                    MaxLength = 200
                },
                new CreateFormFieldDto
                {
                    FieldName = "participant_count",
                    DisplayName = "Katılımcı Sayısı",
                    FieldType = "number",
                    PlaceholderText = "Kaç kişi katılacak?",
                    IsRequired = true,
                    SortOrder = 2,
                    ValidationRules = @"{""min"": 1, ""max"": 100}"
                },
                new CreateFormFieldDto
                {
                    FieldName = "preferred_date",
                    DisplayName = "Tercih Edilen Tarih",
                    FieldType = "date",
                    HelpText = "Eğitimi hangi tarihte almak istersiniz?",
                    IsRequired = false,
                    SortOrder = 3
                },
                new CreateFormFieldDto
                {
                    FieldName = "training_type",
                    DisplayName = "Eğitim Türü",
                    FieldType = "radio",
                    Options = @"[
                        {""value"":""online"",""text"":""Online Eğitim""},
                        {""value"":""onsite"",""text"":""Yerinde Eğitim""},
                        {""value"":""hybrid"",""text"":""Hibrit Eğitim""}
                    ]",
                    IsRequired = true,
                    SortOrder = 4
                }
            };
        }

        public static class FeatureRequest
        {
            public static readonly List<CreateFormFieldDto> Fields = new()
            {
                new CreateFormFieldDto
                {
                    FieldName = "feature_title",
                    DisplayName = "Özellik Başlığı",
                    FieldType = "text",
                    PlaceholderText = "İstediğiniz özelliği kısaca özetleyin",
                    IsRequired = true,
                    SortOrder = 1,
                    MaxLength = 150
                },
                new CreateFormFieldDto
                {
                    FieldName = "feature_description",
                    DisplayName = "Detaylı Açıklama",
                    FieldType = "textarea",
                    PlaceholderText = "Bu özelliğin nasıl çalışmasını istiyorsunuz?",
                    IsRequired = true,
                    SortOrder = 2,
                    MinLength = 50,
                    MaxLength = 2000
                },
                new CreateFormFieldDto
                {
                    FieldName = "business_value",
                    DisplayName = "İş Değeri",
                    FieldType = "textarea",
                    PlaceholderText = "Bu özellik işinize nasıl değer katacak?",
                    HelpText = "Özelliğin size sağlayacağı faydaları açıklayın",
                    IsRequired = false,
                    SortOrder = 3,
                    MaxLength = 1000
                }
            };
        }
    }

    // Alan türleri
    public static class FieldTypes
    {
        public const string TEXT = "text";
        public const string TEXTAREA = "textarea";
        public const string SELECT = "select";
        public const string RADIO = "radio";
        public const string CHECKBOX = "checkbox";
        public const string EMAIL = "email";
        public const string NUMBER = "number";
        public const string DATE = "date";
        public const string FILE = "file";

        public static readonly Dictionary<string, string> DisplayNames = new()
        {
            { TEXT, "Metin" },
            { TEXTAREA, "Çok Satırlı Metin" },
            { SELECT, "Seçim Listesi" },
            { RADIO, "Radyo Buton" },
            { CHECKBOX, "Onay Kutusu" },
            { EMAIL, "Email" },
            { NUMBER, "Sayı" },
            { DATE, "Tarih" },
            { FILE, "Dosya" }
        };
    }

    #endregion
}