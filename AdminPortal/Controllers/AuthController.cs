using AdminPortal.Models;
using AdminPortal.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AdminPortal.Controllers
{
    public class AuthController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IApiService apiService, ILogger<AuthController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // If already logged in, redirect to dashboard
            if (Request.Cookies.ContainsKey("AuthToken"))
            {
                return RedirectToAction("Index", "Admin");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var loginData = new
                {
                    identifier = model.Identifier,
                    password = model.Password
                };

                var response = await _apiService.PostAsync<JsonElement>("api/Auth/login", loginData);

                if (response.ValueKind != null)
                {
                    var jsonObj = response;
                    var success = jsonObj.TryGetProperty("success", out var successProp) && successProp.GetBoolean();

                    if (success)
                    {
                        var accessToken = jsonObj.TryGetProperty("accessToken", out var tokenProp)
                            ? tokenProp.GetString()
                            : null;
                        var refreshToken = jsonObj.TryGetProperty("refreshToken", out var refreshProp)
                            ? refreshProp.GetString()
                            : null;
                        var role = jsonObj.TryGetProperty("role", out var roleProp)
                            ? roleProp.GetString()
                            : null;

                        if (string.IsNullOrEmpty(accessToken))
                        {
                            ModelState.AddModelError("", "Invalid response from server");
                            return View(model);
                        }

                        // Check if user is Admin
                        if (role != "Admin")
                        {
                            ModelState.AddModelError("", "Only administrators can access this portal.");
                            return View(model);
                        }

                        // Store access token in cookie
                        var cookieOptions = new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Strict,
                            Expires = DateTime.UtcNow.AddDays(1)
                        };
                        Response.Cookies.Append("AuthToken", accessToken!, cookieOptions);

                        // Store refresh token in cookie
                        if (!string.IsNullOrEmpty(refreshToken))
                        {
                            var refreshCookieOptions = new CookieOptions
                            {
                                HttpOnly = true,
                                Secure = true,
                                SameSite = SameSiteMode.Strict,
                                Expires = DateTime.UtcNow.AddDays(7)
                            };
                            Response.Cookies.Append("RefreshToken", refreshToken, refreshCookieOptions);
                        }

                        // Also store in session as backup
                        HttpContext.Session.SetString("AuthToken", accessToken!);
                        HttpContext.Session.SetString("UserRole", role!);

                        return RedirectToAction("Index", "Admin");
                    }
                    else
                    {
                        var message = response.TryGetProperty("message", out var msgProp)
                            ? msgProp.GetString()
                            : "Invalid credentials";
                        ModelState.AddModelError("", message ?? "Invalid credentials");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Invalid credentials");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                ModelState.AddModelError("", "An error occurred during login. Please try again.");
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            try
            {
                // Optional: Call the backend logout to revoke refresh token
                await _apiService.PostAsync<object>("api/Auth/logout");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to call backend logout during AdminPortal logout");
            }

            Response.Cookies.Delete("AuthToken");
            Response.Cookies.Delete("RefreshToken");
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}

