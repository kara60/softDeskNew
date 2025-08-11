using System.ComponentModel.DataAnnotations;

namespace TicketSystem.API.DTOs
{
    // Ticket listesi için (sayfalama ile)
    public class TicketListDto
    {
        public Guid Id { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty; // Kısaltılmış
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string CreatedByName { get; set; } = string.Empty;
        public string? AssignedToName { get; set; }
        public string? TicketTypeName { get; set; }
        public string? CategoryName { get; set; }
        public string? ModuleName { get; set; }
    }

    // Ticket detayı için (yorumlar ve eklerle birlikte)
    public class TicketDetailDto
    {
        public Guid Id { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty; // Tam metin
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string CreatedByName { get; set; } = string.Empty;
        public string? AssignedToName { get; set; }
        public string? TicketTypeName { get; set; }
        public string? CategoryName { get; set; }
        public string? ModuleName { get; set; }
        public List<TicketCommentDto> Comments { get; set; } = new List<TicketCommentDto>();
        public List<TicketAttachmentDto> Attachments { get; set; } = new List<TicketAttachmentDto>();
    }

    // Yeni ticket oluşturma için
    public class CreateTicketDto
    {
        [Required(ErrorMessage = "Başlık gereklidir.")]
        [MaxLength(200, ErrorMessage = "Başlık en fazla 200 karakter olabilir.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Açıklama gereklidir.")]
        [MinLength(10, ErrorMessage = "Açıklama en az 10 karakter olmalıdır.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ticket türü seçilmelidir.")]
        public Guid TicketTypeId { get; set; }

        public Guid? CategoryId { get; set; } // Opsiyonel

        public Guid? ModuleId { get; set; } // Opsiyonel

        public string? Priority { get; set; } = "Medium"; // Low, Medium, High, Critical
    }

    // Ticket durumu güncelleme için (Admin/Support only)
    public class UpdateTicketStatusDto
    {
        [Required(ErrorMessage = "Durum gereklidir.")]
        public string Status { get; set; } = string.Empty; // Open, InProgress, Resolved, Closed

        public Guid? AssignedToUserId { get; set; } // Ticket'ı atama
    }

    // Yorum ekleme için
    public class AddCommentDto
    {
        [Required(ErrorMessage = "Yorum gereklidir.")]
        [MinLength(1, ErrorMessage = "Yorum boş olamaz.")]
        public string Comment { get; set; } = string.Empty;

        public bool IsInternal { get; set; } = false; // Sadece support ekibi görsün
    }

    // Yorum gösterimi için
    public class TicketCommentDto
    {
        public Guid Id { get; set; }
        public string Comment { get; set; } = string.Empty;
        public bool IsInternal { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserName { get; set; } = string.Empty;
    }

    // Dosya eki gösterimi için
    public class TicketAttachmentDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Ticket istatistikleri için (Dashboard)
    public class TicketStatsDto
    {
        public int TotalTickets { get; set; }
        public int OpenTickets { get; set; }
        public int InProgressTickets { get; set; }
        public int ResolvedTickets { get; set; }
        public int ClosedTickets { get; set; }
        public int HighPriorityTickets { get; set; }
        public int CriticalPriorityTickets { get; set; }
    }

    // Ticket filtreleme için
    public class TicketFilterDto
    {
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public Guid? TicketTypeId { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? ModuleId { get; set; }
        public Guid? AssignedToUserId { get; set; }
        public DateTime? CreatedFromDate { get; set; }
        public DateTime? CreatedToDate { get; set; }
        public string? SearchTerm { get; set; } // Başlık ve açıklamada arama
    }
}