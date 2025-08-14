// Services/ITicketService.cs
namespace TicketSystem.WEB.Services.Interfaces
{
    public interface ITicketService
    {
        // Ticket Types
        Task<List<TicketTypeDto>> GetTicketTypesAsync();
        Task<TicketTypeDetailDto?> GetTicketTypeDetailAsync(Guid id);

        // Categories
        Task<List<TicketCategoryDto>> GetCategoriesAsync();
        Task<List<TicketModuleDto>> GetCategoryModulesAsync(Guid categoryId);

        // Tickets
        Task<List<TicketListDto>> GetTicketsAsync(TicketFilterDto? filter = null);
        Task<TicketDetailDto?> GetTicketDetailAsync(Guid id);
        Task<CreateTicketResult> CreateTicketAsync(CreateTicketDto ticket);
        Task<bool> UpdateTicketStatusAsync(Guid id, string status);
        Task<bool> AddCommentAsync(Guid ticketId, string comment);

        // Dashboard
        Task<DashboardStatsDto> GetDashboardStatsAsync();
        Task<List<RecentTicketDto>> GetRecentTicketsAsync(int take = 10);
    }

    // DTOs
    public class TicketTypeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public int FormFieldCount { get; set; }
    }

    public class TicketTypeDetailDto : TicketTypeDto
    {
        public List<FormFieldDto> FormFields { get; set; } = new();
    }

    public class FormFieldDto
    {
        public Guid Id { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
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
        public bool IsActive { get; set; }
    }

    public class TicketCategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public string? Color { get; set; }
        public int SortOrder { get; set; }
        public int ModuleCount { get; set; }
    }

    public class TicketModuleDto
    {
        public Guid Id { get; set; }
        public Guid CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public string? Color { get; set; }
        public int SortOrder { get; set; }
    }

    public class TicketListDto
    {
        public Guid Id { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class TicketDetailDto : TicketListDto
    {
        public string Description { get; set; } = string.Empty;
        public List<TicketCommentDto> Comments { get; set; } = new();
        public List<TicketAttachmentDto> Attachments { get; set; } = new();
        public List<TicketFormDataDto> FormData { get; set; } = new();
    }

    public class TicketCommentDto
    {
        public Guid Id { get; set; }
        public string Comment { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class TicketAttachmentDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    public class TicketFormDataDto
    {
        public string FieldName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class CreateTicketDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid TicketTypeId { get; set; }
        public Guid CategoryId { get; set; }
        public Guid? ModuleId { get; set; }
        public string Priority { get; set; } = "NORMAL";
        public List<CreateTicketFormDataDto> FormData { get; set; } = new();
    }

    public class CreateTicketFormDataDto
    {
        public Guid FormFieldId { get; set; }
        public string Value { get; set; } = string.Empty;
    }

    public class CreateTicketResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid? TicketId { get; set; }
        public string? TicketNumber { get; set; }
    }

    public class TicketFilterDto
    {
        public string? Status { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? TypeId { get; set; }
        public Guid? CategoryId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class DashboardStatsDto
    {
        public int TotalTickets { get; set; }
        public int PendingTickets { get; set; }
        public int InProgressTickets { get; set; }
        public int ResolvedTickets { get; set; }
        public int ClosedTickets { get; set; }
        public List<CategoryStatsDto> CategoryStats { get; set; } = new();
    }

    public class CategoryStatsDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public int Count { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    public class RecentTicketDto
    {
        public Guid Id { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}