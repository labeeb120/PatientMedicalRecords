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

        private void SetToken(string token)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return;

            // Update Cookie
            httpContext.Response.Cookies.Append("AuthToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddMinutes(60)
            });

            // Update Session
            httpContext.Session.SetString("AuthToken", token);
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

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && token == null)
            {
                var newToken = await TryRefreshTokenAsync();
                if (!string.IsNullOrEmpty(newToken))
                {
                    request = CreateRequest(HttpMethod.Get, endpoint, newToken);
                    response = await _httpClient.SendAsync(request);
                }
            }
            
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

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && token == null)
            {
                var newToken = await TryRefreshTokenAsync();
                if (!string.IsNullOrEmpty(newToken))
                {
                    request = CreateRequest(HttpMethod.Post, endpoint, newToken);
                    if (data != null)
                    {
                        var json = JsonSerializer.Serialize(data);
                        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                    }
                    response = await _httpClient.SendAsync(request);
                }
            }
            
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

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && token == null)
            {
                var newToken = await TryRefreshTokenAsync();
                if (!string.IsNullOrEmpty(newToken))
                {
                    request = CreateRequest(HttpMethod.Put, endpoint, newToken);
                    if (data != null)
                    {
                        var json = JsonSerializer.Serialize(data);
                        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                    }
                    response = await _httpClient.SendAsync(request);
                }
            }
            
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

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && token == null)
            {
                var newToken = await TryRefreshTokenAsync();
                if (!string.IsNullOrEmpty(newToken))
                {
                    request = CreateRequest(HttpMethod.Delete, endpoint, newToken);
                    response = await _httpClient.SendAsync(request);
                }
            }

            return response.IsSuccessStatusCode;
        }

        private async Task<string?> TryRefreshTokenAsync()
        {
            try
            {
                // Extract refresh token from cookie or session
                var refreshToken = _httpContextAccessor.HttpContext?.Request.Cookies["refreshToken"]
                                ?? _httpContextAccessor.HttpContext?.Session.GetString("refreshToken");

                if (string.IsNullOrEmpty(refreshToken)) return null;

                // We don't use CreateRequest here to avoid infinite loop
                var refreshRequest = new HttpRequestMessage(HttpMethod.Post, "api/Auth/refresh");
                var refreshData = new { RefreshToken = refreshToken };
                var json = JsonSerializer.Serialize(refreshData);
                refreshRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(refreshRequest);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<JsonElement>(content);
                    if (result.TryGetProperty("accessToken", out var tokenProp))
                    {
                        var newToken = tokenProp.GetString();
                        if (!string.IsNullOrEmpty(newToken))
                        {
                            SetToken(newToken);

                            // Also update refresh token if returned (Rotation)
                            if (result.TryGetProperty("refreshToken", out var refreshProp))
                            {
                                var newRefresh = refreshProp.GetString();
                                if (!string.IsNullOrEmpty(newRefresh))
                                {
                                    _httpContextAccessor.HttpContext?.Response.Cookies.Append("refreshToken", newRefresh, new CookieOptions
                                    {
                                        HttpOnly = true,
                                        Secure = true,
                                        SameSite = SameSiteMode.Strict,
                                        Expires = DateTimeOffset.UtcNow.AddDays(30)
                                    });
                                }
                            }

                            return newToken;
                        }
                    }
                }
            }
            catch
            {
                // Log error
            }
            return null;
        }

        public async Task<Stream> GetStreamAsync(string endpoint, string? token = null)
        {
            var request = CreateRequest(HttpMethod.Get, endpoint, token);
            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && token == null)
            {
                var newToken = await TryRefreshTokenAsync();
                if (!string.IsNullOrEmpty(newToken))
                {
                    request = CreateRequest(HttpMethod.Get, endpoint, newToken);
                    response = await _httpClient.SendAsync(request);
                }
            }

            return await response.Content.ReadAsStreamAsync();
        }
    }
}

