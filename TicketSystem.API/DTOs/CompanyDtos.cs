using System.ComponentModel.DataAnnotations;

namespace TicketSystem.API.DTOs
{
    // Şirket listesi için (Admin paneli)
    public class CompanyListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? ContactPerson { get; set; }
        public bool IsActive { get; set; }
        public int TicketCredits { get; set; }
        public string PlanType { get; set; } = string.Empty; // FREE, BASIC, PREMIUM
        public int MonthlyTicketLimit { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UserCount { get; set; } // Şirketteki kullanıcı sayısı
        public int TicketCount { get; set; } // Toplam ticket sayısı
    }

    // Şirket detayı için
    public class CompanyDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? ContactPerson { get; set; }
        public bool IsActive { get; set; }
        public int TicketCredits { get; set; }
        public string PlanType { get; set; } = string.Empty;
        public int MonthlyTicketLimit { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // İstatistikler
        public int UserCount { get; set; }
        public int TicketCount { get; set; }
        public int OpenTicketCount { get; set; }
        public int ResolvedTicketCount { get; set; }
    }

    // Yeni şirket oluşturma için (SuperAdmin only)
    public class CreateCompanyDto
    {
        [Required(ErrorMessage = "Şirket adı gereklidir.")]
        [MaxLength(100, ErrorMessage = "Şirket adı en fazla 100 karakter olabilir.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Database adı gereklidir.")]
        [MaxLength(50, ErrorMessage = "Database adı en fazla 50 karakter olabilir.")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Database adı sadece harf, rakam ve alt çizgi içerebilir.")]
        public string DatabaseName { get; set; } = string.Empty;

        [MaxLength(200, ErrorMessage = "Adres en fazla 200 karakter olabilir.")]
        public string? Address { get; set; }

        [MaxLength(20, ErrorMessage = "Telefon en fazla 20 karakter olabilir.")]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
        [MaxLength(100, ErrorMessage = "Email en fazla 100 karakter olabilir.")]
        public string? Email { get; set; }

        [MaxLength(50, ErrorMessage = "İletişim kişisi en fazla 50 karakter olabilir.")]
        public string? ContactPerson { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Ticket kredisi 0 veya pozitif olmalıdır.")]
        public int TicketCredits { get; set; } = 100; // Default 100 ticket

        public string PlanType { get; set; } = "BASIC"; // FREE, BASIC, PREMIUM

        [Range(1, 10000, ErrorMessage = "Aylık ticket limiti 1-10000 arasında olmalıdır.")]
        public int MonthlyTicketLimit { get; set; } = 50; // Default 50 ticket/month
    }

    // Şirket güncelleme için
    public class UpdateCompanyDto
    {
        [Required(ErrorMessage = "Şirket adı gereklidir.")]
        [MaxLength(100, ErrorMessage = "Şirket adı en fazla 100 karakter olabilir.")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200, ErrorMessage = "Adres en fazla 200 karakter olabilir.")]
        public string? Address { get; set; }

        [MaxLength(20, ErrorMessage = "Telefon en fazla 20 karakter olabilir.")]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
        [MaxLength(100, ErrorMessage = "Email en fazla 100 karakter olabilir.")]
        public string? Email { get; set; }

        [MaxLength(50, ErrorMessage = "İletişim kişisi en fazla 50 karakter olabilir.")]
        public string? ContactPerson { get; set; }

        // SuperAdmin için ek alanlar
        [Range(0, int.MaxValue, ErrorMessage = "Ticket kredisi 0 veya pozitif olmalıdır.")]
        public int TicketCredits { get; set; }

        public string PlanType { get; set; } = string.Empty; // FREE, BASIC, PREMIUM

        [Range(1, 10000, ErrorMessage = "Aylık ticket limiti 1-10000 arasında olmalıdır.")]
        public int MonthlyTicketLimit { get; set; }

        public bool IsActive { get; set; } = true;
    }

    // Kontör ekleme için (SuperAdmin only)
    public class AddCreditsDto
    {
        [Required(ErrorMessage = "Kredi miktarı gereklidir.")]
        [Range(1, 10000, ErrorMessage = "Kredi miktarı 1-10000 arasında olmalıdır.")]
        public int Credits { get; set; }

        [MaxLength(200, ErrorMessage = "Açıklama en fazla 200 karakter olabilir.")]
        public string? Description { get; set; } // "Plan yükseltme", "Ek kredi satışı" vs.
    }

    // Basit şirket bilgisi (dropdown'lar için)
    public class SimpleCompanyDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PlanType { get; set; } = string.Empty;
        public int TicketCredits { get; set; }
        public bool IsActive { get; set; }
    }

    // Şirket istatistikleri için (Dashboard)
    public class CompanyStatsDto
    {
        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalTickets { get; set; }
        public int OpenTickets { get; set; }
        public int InProgressTickets { get; set; }
        public int ResolvedTickets { get; set; }
        public int ClosedTickets { get; set; }
        public int TicketCredits { get; set; }
        public int MonthlyTicketUsage { get; set; } // Bu ay kullanılan ticket sayısı
        public string PlanType { get; set; } = string.Empty;
    }

    // Plan türleri
    public static class PlanTypes
    {
        public const string FREE = "FREE";
        public const string BASIC = "BASIC";
        public const string PREMIUM = "PREMIUM";
        public const string ENTERPRISE = "ENTERPRISE";

        public static readonly Dictionary<string, PlanInfo> Plans = new()
        {
            {
                FREE,
                new PlanInfo
                {
                    Name = "Ücretsiz",
                    MonthlyTicketLimit = 10,
                    UserLimit = 3,
                    Features = new[] { "Temel ticket sistemi", "Email desteği" }
                }
            },
            {
                BASIC,
                new PlanInfo
                {
                    Name = "Temel",
                    MonthlyTicketLimit = 50,
                    UserLimit = 10,
                    Features = new[] { "Gelişmiş ticket sistemi", "Dosya ekleme", "Yorum sistemi" }
                }
            },
            {
                PREMIUM,
                new PlanInfo
                {
                    Name = "Premium",
                    MonthlyTicketLimit = 200,
                    UserLimit = 50,
                    Features = new[] { "Tüm özellikler", "PMO entegrasyonu", "Öncelikli destek" }
                }
            },
            {
                ENTERPRISE,
                new PlanInfo
                {
                    Name = "Kurumsal",
                    MonthlyTicketLimit = -1, // Sınırsız
                    UserLimit = -1, // Sınırsız
                    Features = new[] { "Sınırsız kullanım", "Özel entegrasyonlar", "24/7 destek" }
                }
            }
        };
    }

    // Plan bilgileri
    public class PlanInfo
    {
        public string Name { get; set; } = string.Empty;
        public int MonthlyTicketLimit { get; set; } // -1 = sınırsız
        public int UserLimit { get; set; } // -1 = sınırsız
        public string[] Features { get; set; } = Array.Empty<string>();
    }
}