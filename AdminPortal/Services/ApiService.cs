using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AdminPortal.Services
{
    public interface IApiService
    {
        Task<T?> GetAsync<T>(string endpoint, string? token = null);
        Task<T?> PostAsync<T>(string endpoint, object? data = null, string? token = null);
        Task<T?> PutAsync<T>(string endpoint, object? data = null, string? token = null);
        Task<bool> DeleteAsync(string endpoint, string? token = null);
        Task<Stream> GetStreamAsync(string endpoint, string? token = null);
    }

    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            
            var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7164";
            _httpClient.BaseAddress = new Uri(apiBaseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private string? GetToken()
        {
            return _httpContextAccessor.HttpContext?.Request.Cookies["AuthToken"] 
                ?? _httpContextAccessor.HttpContext?.Session.GetString("AuthToken");
        }

        private HttpRequestMessage CreateRequest(HttpMethod method, string endpoint, string? token = null)
        {
            var tokenToUse = token ?? GetToken();
            var request = new HttpRequestMessage(method, endpoint);
            
            if (!string.IsNullOrEmpty(tokenToUse))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenToUse);
            }
            
            return request;
        }

        public async Task<T?> GetAsync<T>(string endpoint, string? token = null)
        {
            var request = CreateRequest(HttpMethod.Get, endpoint, token);
            var response = await _httpClient.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });
            }
            
            return default;
        }

        public async Task<T?> PostAsync<T>(string endpoint, object? data = null, string? token = null)
        {
            var request = CreateRequest(HttpMethod.Post, endpoint, token);
            if (data != null)
            {
                var json = JsonSerializer.Serialize(data);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }
            
            var response = await _httpClient.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });
            }
            
            return default;
        }

        public async Task<T?> PutAsync<T>(string endpoint, object? data = null, string? token = null)
        {
            var request = CreateRequest(HttpMethod.Put, endpoint, token);
            if (data != null)
            {
                var json = JsonSerializer.Serialize(data);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }
            
            var response = await _httpClient.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });
            }
            
            return default;
        }

        public async Task<bool> DeleteAsync(string endpoint, string? token = null)
        {
            var request = CreateRequest(HttpMethod.Delete, endpoint, token);
            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<Stream> GetStreamAsync(string endpoint, string? token = null)
        {
            var request = CreateRequest(HttpMethod.Get, endpoint, token);
            var response = await _httpClient.SendAsync(request);
            return await response.Content.ReadAsStreamAsync();
        }
    }
}

