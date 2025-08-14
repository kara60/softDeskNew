// Services/IAuthService.cs
using Microsoft.JSInterop;
using System.Text.Json;

namespace TicketSystem.WEB.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> LoginAsync(string username, string password, bool rememberMe = false);
        Task LogoutAsync();
        Task<bool> IsAuthenticatedAsync();
        Task<CurrentUser?> GetCurrentUserAsync();
        Task<string?> GetTokenAsync();
    }

    

    // DTOs
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public CurrentUser User { get; set; } = new();
        public DateTime ExpiresAt { get; set; }
    }

    public class CurrentUser
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public List<string> Roles { get; set; } = new();
        public CompanyInfo? Company { get; set; }
    }

    public class CompanyInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
    }

    public class AuthResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}