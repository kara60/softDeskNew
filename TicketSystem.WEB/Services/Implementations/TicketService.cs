using TicketSystem.WEB.Services.Interfaces;

namespace TicketSystem.WEB.Services.Implementations
{
    public class TicketService : ITicketService
    {
        private readonly IApiService _apiService;

        public TicketService(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<List<TicketTypeDto>> GetTicketTypesAsync()
        {
            var result = await _apiService.GetAsync<List<TicketTypeDto>>("api/tickettypes");
            return result ?? new List<TicketTypeDto>();
        }

        public async Task<TicketTypeDetailDto?> GetTicketTypeDetailAsync(Guid id)
        {
            return await _apiService.GetAsync<TicketTypeDetailDto>($"api/tickettypes/{id}");
        }

        public async Task<List<TicketCategoryDto>> GetCategoriesAsync()
        {
            var result = await _apiService.GetAsync<List<TicketCategoryDto>>("api/tickettypes/categories");
            return result ?? new List<TicketCategoryDto>();
        }

        public async Task<List<TicketModuleDto>> GetCategoryModulesAsync(Guid categoryId)
        {
            var result = await _apiService.GetAsync<List<TicketModuleDto>>($"api/tickettypes/categories/{categoryId}/modules");
            return result ?? new List<TicketModuleDto>();
        }

        public async Task<List<TicketListDto>> GetTicketsAsync(TicketFilterDto? filter = null)
        {
            var endpoint = "api/tickets";

            if (filter != null)
            {
                var queryParams = new List<string>();

                if (!string.IsNullOrEmpty(filter.Status))
                    queryParams.Add($"status={filter.Status}");

                if (filter.CompanyId.HasValue)
                    queryParams.Add($"companyId={filter.CompanyId}");

                if (filter.TypeId.HasValue)
                    queryParams.Add($"typeId={filter.TypeId}");

                if (queryParams.Any())
                    endpoint += "?" + string.Join("&", queryParams);
            }

            var result = await _apiService.GetAsync<List<TicketListDto>>(endpoint);
            return result ?? new List<TicketListDto>();
        }

        public async Task<TicketDetailDto?> GetTicketDetailAsync(Guid id)
        {
            return await _apiService.GetAsync<TicketDetailDto>($"api/tickets/{id}");
        }

        public async Task<CreateTicketResult> CreateTicketAsync(CreateTicketDto ticket)
        {
            var result = await _apiService.PostAsync<CreateTicketResult>("api/tickets", ticket);
            return result ?? new CreateTicketResult { Success = false, Message = "Ticket oluşturulamadı" };
        }

        public async Task<bool> UpdateTicketStatusAsync(Guid id, string status)
        {
            var result = await _apiService.PutAsync<bool>($"api/tickets/{id}/status", new { status });
            return result;
        }

        public async Task<bool> AddCommentAsync(Guid ticketId, string comment)
        {
            var result = await _apiService.PostAsync<bool>($"api/tickets/{ticketId}/comments", new { comment });
            return result;
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            var result = await _apiService.GetAsync<DashboardStatsDto>("api/tickets/dashboard/stats");
            return result ?? new DashboardStatsDto();
        }

        public async Task<List<RecentTicketDto>> GetRecentTicketsAsync(int take = 10)
        {
            var result = await _apiService.GetAsync<List<RecentTicketDto>>($"api/tickets/recent?take={take}");
            return result ?? new List<RecentTicketDto>();
        }
    }
}