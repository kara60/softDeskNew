using Microsoft.JSInterop;
using TicketSystem.WEB.Services.Interfaces;

namespace TicketSystem.WEB.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IApiService _apiService;
        private readonly IJSRuntime _jsRuntime;
        private CurrentUser? _currentUser;

        public AuthService(IApiService apiService, IJSRuntime jsRuntime)
        {
            _apiService = apiService;
            _jsRuntime = jsRuntime;
        }

        public async Task<AuthResult> LoginAsync(string username, string password, bool rememberMe = false)
        {
            try
            {
                var loginRequest = new
                {
                    Username = username,
                    Password = password,
                    RememberMe = rememberMe
                };

                var response = await _apiService.PostAsync<LoginResponse>("api/auth/login", loginRequest);

                if (response != null && !string.IsNullOrEmpty(response.Token))
                {
                    // Token'ı kaydet
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", response.Token);

                    // API service'e token'ı set et
                    _apiService.SetAuthToken(response.Token);

                    // Kullanıcı bilgilerini kaydet
                    _currentUser = response.User;

                    return new AuthResult { Success = true, Message = "Giriş başarılı" };
                }

                return new AuthResult { Success = false, Message = "Kullanıcı adı veya şifre hatalı" };
            }
            catch (Exception ex)
            {
                return new AuthResult { Success = false, Message = $"Giriş hatası: {ex.Message}" };
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                // Token'ı temizle
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");

                // API service'den token'ı kaldır
                _apiService.ClearAuthToken();

                // Kullanıcı bilgilerini temizle
                _currentUser = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logout error: {ex.Message}");
            }
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            try
            {
                var token = await GetTokenAsync();

                if (string.IsNullOrEmpty(token))
                    return false;

                // Token'ın geçerliliğini kontrol et
                var user = await GetCurrentUserAsync();
                return user != null;
            }
            catch
            {
                return false;
            }
        }

        public async Task<CurrentUser?> GetCurrentUserAsync()
        {
            try
            {
                if (_currentUser != null)
                    return _currentUser;

                var token = await GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                    return null;

                _apiService.SetAuthToken(token);

                // API'den kullanıcı bilgilerini al
                _currentUser = await _apiService.GetAsync<CurrentUser>("api/auth/me");

                return _currentUser;
            }
            catch
            {
                return null;
            }
        }

        public async Task<string?> GetTokenAsync()
        {
            try
            {
                return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
            }
            catch
            {
                return null;
            }
        }
    }
}