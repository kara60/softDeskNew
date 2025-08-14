// Services/Interfaces/IApiService.cs
namespace TicketSystem.WEB.Services.Interfaces
{
    public interface IApiService
    {
        Task<T?> GetAsync<T>(string endpoint);
        Task<T?> PostAsync<T>(string endpoint, object data);
        Task<T?> PutAsync<T>(string endpoint, object data);
        Task<bool> DeleteAsync(string endpoint);
        Task<string?> PostFileAsync(string endpoint, MultipartFormDataContent content);
        void SetAuthToken(string token);
        void ClearAuthToken();
    }
}