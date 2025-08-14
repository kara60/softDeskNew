// Services/Implementations/ApiService.cs
using System.Text.Json;
using TicketSystem.WEB.Services.Interfaces;

namespace TicketSystem.WEB.Services.Implementations
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("TicketAPI");
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<T>(json, _jsonOptions);
                }

                // Log error for debugging
                Console.WriteLine($"API GET Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                return default(T);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API GET Exception: {ex.Message}");
                return default(T);
            }
        }

        public async Task<T?> PostAsync<T>(string endpoint, object data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(endpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<T>(responseJson, _jsonOptions);
                }

                // Log error for debugging
                Console.WriteLine($"API POST Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                return default(T);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API POST Exception: {ex.Message}");
                return default(T);
            }
        }

        public async Task<T?> PutAsync<T>(string endpoint, object data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(endpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<T>(responseJson, _jsonOptions);
                }

                return default(T);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API PUT Exception: {ex.Message}");
                return default(T);
            }
        }

        public async Task<bool> DeleteAsync(string endpoint)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(endpoint);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API DELETE Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<string?> PostFileAsync(string endpoint, MultipartFormDataContent content)
        {
            try
            {
                var response = await _httpClient.PostAsync(endpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API FILE POST Exception: {ex.Message}");
                return null;
            }
        }

        public void SetAuthToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        public void ClearAuthToken()
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }
}