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
            
            var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://192.168.1.101:5137";
            _httpClient.BaseAddress = new Uri(apiBaseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        //private string? GetToken()
        //{
        //    return _httpContextAccessor.HttpContext?.Request.Cookies["AuthToken"] 
        //        ?? _httpContextAccessor.HttpContext?.Session.GetString("AuthToken");
        //}
        private string? GetToken()
        {
            // Prioritize the Session-based token, as it is updated immediately during RefreshTokenAsync, 
            // whereas Request.Cookies is immutable for the duration of the current request.
            return _httpContextAccessor.HttpContext?.Session.GetString("AuthToken")
                ?? _httpContextAccessor.HttpContext?.Request.Cookies["AuthToken"];
        }

        private string? GetRefreshToken()
        {
            return _httpContextAccessor.HttpContext?.Request.Cookies["RefreshToken"];
        }

        private async Task<bool> RefreshTokenAsync()
        {
            var refreshToken = GetRefreshToken();
            if (string.IsNullOrEmpty(refreshToken)) return false;

            var refreshData = new { refreshToken = refreshToken };
            var json = JsonSerializer.Serialize(refreshData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/Auth/refresh", content);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(responseContent);

                if (result.TryGetProperty("success", out var successProp) && successProp.GetBoolean())
                {
                    var newAccessToken = result.TryGetProperty("accessToken", out var tokenProp) ? tokenProp.GetString() : null;
                    if (!string.IsNullOrEmpty(newAccessToken))
                    {
                        var cookieOptions = new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Strict,
                            Expires = DateTime.UtcNow.AddDays(1)
                        };
                        _httpContextAccessor.HttpContext?.Response.Cookies.Append("AuthToken", newAccessToken, cookieOptions);
                        _httpContextAccessor.HttpContext?.Session.SetString("AuthToken", newAccessToken);

                        // If a new refresh token was also returned, update it
                        if (result.TryGetProperty("newRefreshToken", out var newRefreshProp))
                        {
                            var newRefreshToken = newRefreshProp.GetString();
                            if (!string.IsNullOrEmpty(newRefreshToken))
                            {
                                var refreshCookieOptions = new CookieOptions
                                {
                                    HttpOnly = true,
                                    Secure = true,
                                    SameSite = SameSiteMode.Strict,
                                    Expires = DateTime.UtcNow.AddDays(7)
                                };
                                _httpContextAccessor.HttpContext?.Response.Cookies.Append("RefreshToken", newRefreshToken, refreshCookieOptions);
                            }
                        }

                        return true;
                    }
                }
            }
            return false;
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
                if (await RefreshTokenAsync())
                {
                    request = CreateRequest(HttpMethod.Get, endpoint);
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
            Func<HttpRequestMessage> createReq = () => {
                var req = CreateRequest(HttpMethod.Post, endpoint, token);
                if (data != null)
                {
                    var json = JsonSerializer.Serialize(data);
                    req.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }
                return req;
            };

            var response = await _httpClient.SendAsync(createReq());

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && token == null)
            {
                if (await RefreshTokenAsync())
                {
                    response = await _httpClient.SendAsync(createReq());
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
            Func<HttpRequestMessage> createReq = () => {
                var req = CreateRequest(HttpMethod.Put, endpoint, token);
                if (data != null)
                {
                    var json = JsonSerializer.Serialize(data);
                    req.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }
                return req;
            };

            var response = await _httpClient.SendAsync(createReq());

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && token == null)
            {
                if (await RefreshTokenAsync())
                {
                    response = await _httpClient.SendAsync(createReq());
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
                if (await RefreshTokenAsync())
                {
                    request = CreateRequest(HttpMethod.Delete, endpoint);
                    response = await _httpClient.SendAsync(request);
                }
            }
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

