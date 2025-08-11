using System.ComponentModel.DataAnnotations;

namespace TicketSystem.API.DTOs
{
    #region System Settings DTOs

    // Sistem ayarı bilgisi
    public class SystemSettingDto
    {
        public Guid Id { get; set; }
        public string SettingKey { get; set; } = string.Empty;
        public string SettingValue { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty; // string, boolean, number, json
        public string? DisplayName { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public bool IsSystemSetting { get; set; }
        public string? DefaultValue { get; set; }
        public string? ValidationRules { get; set; }
    }

    // Ayar güncelleme için
    public class UpdateSettingDto
    {
        [Required(ErrorMessage = "Ayar değeri gereklidir.")]
        public string SettingValue { get; set; } = string.Empty;
    }

    // Yeni ayar oluşturma için
    public class CreateSettingDto
    {
        [Required(ErrorMessage = "Ayar anahtarı gereklidir.")]
        [MaxLength(100, ErrorMessage = "Ayar anahtarı en fazla 100 karakter olabilir.")]
        [RegularExpression(@"^[A-Z][A-Z0-9_]*$", ErrorMessage = "Ayar anahtarı büyük harf ile başlamalı ve sadece büyük harf, rakam, alt çizgi içerebilir.")]
        public string SettingKey { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ayar değeri gereklidir.")]
        public string SettingValue { get; set; } = string.Empty;

        [Required(ErrorMessage = "Veri türü gereklidir.")]
        public string DataType { get; set; } = "string"; // string, boolean, number, json

        [MaxLength(200, ErrorMessage = "Görünen ad en fazla 200 karakter olabilir.")]
        public string? DisplayName { get; set; }

        [MaxLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        public string? Description { get; set; }

        [MaxLength(50, ErrorMessage = "Kategori en fazla 50 karakter olabilir.")]
        public string? Category { get; set; } = "General";

        public bool IsVisible { get; set; } = true;
        public string? DefaultValue { get; set; }
        public string? ValidationRules { get; set; }
    }

    // Bağlantı testi için
    public class TestConnectionDto
    {
        [Required(ErrorMessage = "Bağlantı türü gereklidir.")]
        public string ConnectionType { get; set; } = string.Empty; // email, pmo, database

        [Required(ErrorMessage = "Ayarlar gereklidir.")]
        public Dictionary<string, string> Settings { get; set; } = new Dictionary<string, string>();
    }

    #endregion

    #region Predefined Settings

    // Önceden tanımlanmış sistem ayarları
    public static class DefaultSystemSettings
    {
        public static readonly List<CreateSettingDto> Settings = new()
        {
            #region PMO Integration Settings
            new CreateSettingDto
            {
                SettingKey = "PMO_INTEGRATION_ENABLED",
                SettingValue = "false",
                DataType = "boolean",
                DisplayName = "PMO Entegrasyonu Aktif",
                Description = "Ticket onaylandığında otomatik olarak PMO'ya iş oluşturulsun",
                Category = "PMO",
                DefaultValue = "false"
            },
            new CreateSettingDto
            {
                SettingKey = "PMO_API_ENDPOINT",
                SettingValue = "https://your-pmo-system.com/api",
                DataType = "string",
                DisplayName = "PMO API Endpoint",
                Description = "PMO sisteminin API URL'i",
                Category = "PMO",
                ValidationRules = @"{""pattern"": ""url""}"
            },
            new CreateSettingDto
            {
                SettingKey = "PMO_AUTH_TOKEN",
                SettingValue = "",
                DataType = "string",
                DisplayName = "PMO Yetkilendirme Token",
                Description = "PMO API'sine erişim için token",
                Category = "PMO"
            },
            new CreateSettingDto
            {
                SettingKey = "PMO_TIMEOUT_SECONDS",
                SettingValue = "30",
                DataType = "number",
                DisplayName = "PMO Bağlantı Timeout (Saniye)",
                Description = "PMO API'si için maksimum bekleme süresi",
                Category = "PMO",
                DefaultValue = "30",
                ValidationRules = @"{""min"": 5, ""max"": 300}"
            },
            #endregion

            #region Email Settings
            new CreateSettingDto
            {
                SettingKey = "EMAIL_ENABLED",
                SettingValue = "true",
                DataType = "boolean",
                DisplayName = "Email Bildirimleri Aktif",
                Description = "Sistem email bildirimleri göndersin",
                Category = "Email",
                DefaultValue = "true"
            },
            new CreateSettingDto
            {
                SettingKey = "SMTP_HOST",
                SettingValue = "smtp.gmail.com",
                DataType = "string",
                DisplayName = "SMTP Sunucu",
                Description = "Email göndermek için SMTP sunucu adresi",
                Category = "Email"
            },
            new CreateSettingDto
            {
                SettingKey = "SMTP_PORT",
                SettingValue = "587",
                DataType = "number",
                DisplayName = "SMTP Port",
                Description = "SMTP sunucu port numarası",
                Category = "Email",
                DefaultValue = "587",
                ValidationRules = @"{""min"": 1, ""max"": 65535}"
            },
            new CreateSettingDto
            {
                SettingKey = "SMTP_USERNAME",
                SettingValue = "",
                DataType = "string",
                DisplayName = "SMTP Kullanıcı Adı",
                Description = "SMTP kimlik doğrulama için kullanıcı adı",
                Category = "Email"
            },
            new CreateSettingDto
            {
                SettingKey = "SMTP_PASSWORD",
                SettingValue = "",
                DataType = "string",
                DisplayName = "SMTP Şifre",
                Description = "SMTP kimlik doğrulama için şifre",
                Category = "Email"
            },
            new CreateSettingDto
            {
                SettingKey = "EMAIL_FROM",
                SettingValue = "noreply@softticket.com",
                DataType = "string",
                DisplayName = "Gönderen Email",
                Description = "Sistem emaillerinin gönderen adresi",
                Category = "Email",
                ValidationRules = @"{""pattern"": ""email""}"
            },
            new CreateSettingDto
            {
                SettingKey = "EMAIL_FROM_NAME",
                SettingValue = "SoftTicket Sistemi",
                DataType = "string",
                DisplayName = "Gönderen Adı",
                Description = "Sistem emaillerinin gönderen adı",
                Category = "Email",
                DefaultValue = "SoftTicket Sistemi"
            },
            #endregion

            #region File Upload Settings
            new CreateSettingDto
            {
                SettingKey = "FILE_UPLOAD_ENABLED",
                SettingValue = "true",
                DataType = "boolean",
                DisplayName = "Dosya Yükleme Aktif",
                Description = "Kullanıcılar ticket'lara dosya ekleyebilsin",
                Category = "FileUpload",
                DefaultValue = "true"
            },
            new CreateSettingDto
            {
                SettingKey = "MAX_FILE_SIZE_MB",
                SettingValue = "10",
                DataType = "number",
                DisplayName = "Maksimum Dosya Boyutu (MB)",
                Description = "Yüklenebilecek maksimum dosya boyutu",
                Category = "FileUpload",
                DefaultValue = "10",
                ValidationRules = @"{""min"": 1, ""max"": 100}"
            },
            new CreateSettingDto
            {
                SettingKey = "ALLOWED_FILE_TYPES",
                SettingValue = "jpg,jpeg,png,gif,pdf,doc,docx,xls,xlsx,txt,zip,rar",
                DataType = "string",
                DisplayName = "İzin Verilen Dosya Türleri",
                Description = "Yüklenebilecek dosya uzantıları (virgülle ayrılmış)",
                Category = "FileUpload",
                DefaultValue = "jpg,jpeg,png,gif,pdf,doc,docx,txt"
            },
            new CreateSettingDto
            {
                SettingKey = "UPLOAD_PATH",
                SettingValue = "wwwroot/uploads",
                DataType = "string",
                DisplayName = "Yükleme Klasörü",
                Description = "Dosyaların kaydedileceği klasör",
                Category = "FileUpload",
                DefaultValue = "wwwroot/uploads"
            },
            #endregion

            #region Ticket Settings
            new CreateSettingDto
            {
                SettingKey = "TICKET_NUMBER_FORMAT",
                SettingValue = "TK-{YEAR}-{MONTH:D2}-{COUNTER:D3}",
                DataType = "string",
                DisplayName = "Ticket Numara Formatı",
                Description = "Ticket numaralarının oluşturma formatı",
                Category = "Tickets",
                DefaultValue = "TK-{YEAR}-{MONTH:D2}-{COUNTER:D3}"
            },
            new CreateSettingDto
            {
                SettingKey = "AUTO_ASSIGN_TICKETS",
                SettingValue = "false",
                DataType = "boolean",
                DisplayName = "Otomatik Ticket Atama",
                Description = "Yeni ticket'lar otomatik olarak support'a atansın",
                Category = "Tickets",
                DefaultValue = "false"
            },
            new CreateSettingDto
            {
                SettingKey = "TICKET_AUTO_CLOSE_DAYS",
                SettingValue = "30",
                DataType = "number",
                DisplayName = "Otomatik Kapatma (Gün)",
                Description = "Çözülmüş ticket'lar kaç gün sonra otomatik kapansın (0=kapatma)",
                Category = "Tickets",
                DefaultValue = "30",
                ValidationRules = @"{""min"": 0, ""max"": 365}"
            },
            new CreateSettingDto
            {
                SettingKey = "REQUIRE_TICKET_APPROVAL",
                SettingValue = "false",
                DataType = "boolean",
                DisplayName = "Ticket Onayı Gerekli",
                Description = "Ticket'lar işleme alınmadan önce admin onayı gereksin",
                Category = "Tickets",
                DefaultValue = "false"
            },
            #endregion

            #region Security Settings
            new CreateSettingDto
            {
                SettingKey = "SESSION_TIMEOUT_MINUTES",
                SettingValue = "60",
                DataType = "number",
                DisplayName = "Oturum Timeout (Dakika)",
                Description = "Kullanıcı oturumlarının otomatik sonlanma süresi",
                Category = "Security",
                DefaultValue = "60",
                ValidationRules = @"{""min"": 15, ""max"": 480}"
            },
            new CreateSettingDto
            {
                SettingKey = "PASSWORD_MIN_LENGTH",
                SettingValue = "6",
                DataType = "number",
                DisplayName = "Minimum Şifre Uzunluğu",
                Description = "Kullanıcı şifrelerinin minimum karakter sayısı",
                Category = "Security",
                DefaultValue = "6",
                ValidationRules = @"{""min"": 4, ""max"": 50}"
            },
            new CreateSettingDto
            {
                SettingKey = "REQUIRE_STRONG_PASSWORD",
                SettingValue = "true",
                DataType = "boolean",
                DisplayName = "Güçlü Şifre Zorunlu",
                Description = "Şifrelerde büyük/küçük harf, rakam zorunluluğu",
                Category = "Security",
                DefaultValue = "true"
            },
            #endregion

            #region Notification Settings
            new CreateSettingDto
            {
                SettingKey = "NOTIFY_ON_TICKET_CREATE",
                SettingValue = "true",
                DataType = "boolean",
                DisplayName = "Ticket Oluşturma Bildirimi",
                Description = "Yeni ticket oluşturulduğunda email gönder",
                Category = "Notifications",
                DefaultValue = "true"
            },
            new CreateSettingDto
            {
                SettingKey = "NOTIFY_ON_STATUS_CHANGE",
                SettingValue = "true",
                DataType = "boolean",
                DisplayName = "Durum Değişikliği Bildirimi",
                Description = "Ticket durumu değiştiğinde email gönder",
                Category = "Notifications",
                DefaultValue = "true"
            },
            new CreateSettingDto
            {
                SettingKey = "NOTIFY_ON_COMMENT",
                SettingValue = "true",
                DataType = "boolean",
                DisplayName = "Yorum Bildirimi",
                Description = "Ticket'a yorum eklendiğinde email gönder",
                Category = "Notifications",
                DefaultValue = "true"
            },
            #endregion

            #region System Settings
            new CreateSettingDto
            {
                SettingKey = "SYSTEM_MAINTENANCE_MODE",
                SettingValue = "false",
                DataType = "boolean",
                DisplayName = "Bakım Modu",
                Description = "Sistem bakım modunda olsun (sadece admin erişir)",
                Category = "System",
                DefaultValue = "false"
            },
            new CreateSettingDto
            {
                SettingKey = "SYSTEM_TIMEZONE",
                SettingValue = "Turkey Standard Time",
                DataType = "string",
                DisplayName = "Sistem Saat Dilimi",
                Description = "Sistem genelinde kullanılacak saat dilimi",
                Category = "System",
                DefaultValue = "Turkey Standard Time"
            },
            new CreateSettingDto
            {
                SettingKey = "LOG_LEVEL",
                SettingValue = "Information",
                DataType = "string",
                DisplayName = "Log Seviyesi",
                Description = "Sistem log kayıt seviyesi",
                Category = "System",
                DefaultValue = "Information",
                ValidationRules = @"{""enum"": [""Debug"", ""Information"", ""Warning"", ""Error"", ""Critical""]}"
            }
            #endregion
        };
    }

    // Ayar kategorileri
    public static class SettingCategories
    {
        public const string PMO = "PMO";
        public const string EMAIL = "Email";
        public const string FILE_UPLOAD = "FileUpload";
        public const string TICKETS = "Tickets";
        public const string SECURITY = "Security";
        public const string NOTIFICATIONS = "Notifications";
        public const string SYSTEM = "System";
        public const string GENERAL = "General";

        public static readonly Dictionary<string, string> DisplayNames = new()
        {
            { PMO, "PMO Entegrasyonu" },
            { EMAIL, "Email Ayarları" },
            { FILE_UPLOAD, "Dosya Yükleme" },
            { TICKETS, "Ticket Ayarları" },
            { SECURITY, "Güvenlik" },
            { NOTIFICATIONS, "Bildirimler" },
            { SYSTEM, "Sistem" },
            { GENERAL, "Genel" }
        };
    }

    // Veri türleri
    public static class DataTypes
    {
        public const string STRING = "string";
        public const string BOOLEAN = "boolean";
        public const string NUMBER = "number";
        public const string JSON = "json";

        public static readonly Dictionary<string, string> DisplayNames = new()
        {
            { STRING, "Metin" },
            { BOOLEAN, "Evet/Hayır" },
            { NUMBER, "Sayı" },
            { JSON, "JSON Objesi" }
        };
    }

    #endregion
}