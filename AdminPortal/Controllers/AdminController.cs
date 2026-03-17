using AdminPortal.Models;
using AdminPortal.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AdminPortal.Controllers
{
    public class AdminController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IApiService apiService, ILogger<AdminController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        private bool IsAuthenticated()
        {
            return Request.Cookies.ContainsKey("AuthToken") ||
                   !string.IsNullOrEmpty(HttpContext.Session.GetString("AuthToken"));
        }

        private void CheckAuthentication()
        {
            if (!IsAuthenticated())
            {
                Response.Redirect("/Auth/Login");
            }
        }

        // Dashboard
        public async Task<IActionResult> Index()
        {
            CheckAuthentication();
            var statistics = await _apiService.GetAsync<StatisticsResponse>("api/Admin/statistics");
            return View(statistics?.Statistics);
        }

        // Pending Registrations
        public async Task<IActionResult> PendingRegistrations()
        {
            CheckAuthentication();
            var response = await _apiService.GetAsync<JsonElement>("api/Admin/pending-registrations");

            var registrations = new List<PendingRegistration>();
            if (response.ValueKind == JsonValueKind.Array && response.GetArrayLength() > 0)
            {
                registrations = JsonSerializer.Deserialize<List<PendingRegistration>>(
                    response.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<PendingRegistration>();
            }

            return View(registrations);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveRegistration(int userId)
        {
            CheckAuthentication();

            JsonElement result =
                await _apiService.PostAsync<JsonElement>($"api/Admin/approve/{userId}");
            bool isApproved = false;

            if (result.ValueKind == JsonValueKind.Object &&
                result.TryGetProperty("success", out var successProp) &&
                successProp.ValueKind == JsonValueKind.True)
            {
                isApproved = true;
            }

            TempData[isApproved ? "SuccessMessage" : "ErrorMessage"] =
                isApproved ? "Registration approved successfully" : "Failed to approve registration";


            return RedirectToAction("PendingRegistrations");
        }


        [HttpPost]
        public async Task<IActionResult> RejectRegistration(int userId)
        {
            CheckAuthentication();

            JsonElement result =
                await _apiService.PostAsync<JsonElement>($"api/Admin/reject/{userId}");

            if (result.ValueKind == JsonValueKind.Object &&
                result.TryGetProperty("success", out var success) &&
                success.GetBoolean())
            {
                TempData["SuccessMessage"] = "Registration rejected successfully";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to reject registration";
            }

            return RedirectToAction("PendingRegistrations");
        }

        // Users Management
        public async Task<IActionResult> Users(UserRole? role = null, UserStatus? status = null, int pageNumber = 1, int pageSize = 10)
        {
            CheckAuthentication();
            var request = new UserListRequest
            {
                Role = role,
                Status = status,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var response = await _apiService.PostAsync<UserListResponse>("api/Admin/GetUsers", request);
            return View(response);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUserStatus(UserStatusUpdateRequest request)
        {
            CheckAuthentication();
            var response = await _apiService.PutAsync<UserStatusUpdateResponse>("api/Admin/user-status", request);

            if (response?.Success == true)
            {
                TempData["SuccessMessage"] = response.Message;
            }
            else
            {
                TempData["ErrorMessage"] = response?.Message ?? "Failed to update user status";
            }

            return RedirectToAction("Users");
        }

        // Audit Logs
        public async Task<IActionResult> AuditLogs(int? userId = null, DateTime? startDate = null, DateTime? endDate = null, int pageNumber = 1, int pageSize = 10)
        {
            CheckAuthentication();
            var request = new AuditLogRequest
            {
                UserId = userId,
                StartDate = startDate,
                EndDate = endDate,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var response = await _apiService.PostAsync<AuditLogResponse>("api/Admin/audit-logs", request);
            return View(response);
        }

        // Send Notification
        [HttpGet]
        public IActionResult SendNotification()
        {
            CheckAuthentication();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendNotification(string userIds, string subject, string content)
        {
            CheckAuthentication();

            if (string.IsNullOrWhiteSpace(userIds) || string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(content))
            {
                ModelState.AddModelError("", "All fields are required");
                return View(new NotificationRequest { UserIds = new List<int>(), Subject = subject ?? "", Content = content ?? "" });
            }

            // Parse comma-separated user IDs
            var userIdList = userIds.Split(',')
                .Select(id => id.Trim())
                .Where(id => !string.IsNullOrWhiteSpace(id) && int.TryParse(id, out _))
                .Select(int.Parse)
                .ToList();

            if (!userIdList.Any())
            {
                ModelState.AddModelError("", "Please provide valid user IDs");
                return View(new NotificationRequest { UserIds = new List<int>(), Subject = subject, Content = content });
            }

            var request = new NotificationRequest
            {
                UserIds = userIdList,
                Subject = subject,
                Content = content
            };

            var response = await _apiService.PostAsync<NotificationResponse>("api/Admin/send-notification", request);

            if (response?.Success == true)
            {
                TempData["SuccessMessage"] = $"Notification sent successfully. Sent: {response.SentCount}, Failed: {response.FailedCount}";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["ErrorMessage"] = response?.Message ?? "Failed to send notification";
                return View(request);
            }
        }

        // ==================== Bulk Import: Drugs ====================
        [HttpGet]
        public IActionResult ImportDrugs()
        {
            CheckAuthentication();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportDrugs(IFormFile? file, string? jsonText)
        {
            CheckAuthentication();

            if ((file == null || file.Length == 0) && string.IsNullOrWhiteSpace(jsonText))
            {
                TempData["ErrorMessage"] = "يرجى اختيار ملف JSON أو لصق محتوى JSON.";
                return View();
            }

            try
            {
                List<DrugImportDto> drugs;

                if (file != null && file.Length > 0)
                {
                    using var stream = file.OpenReadStream();
                    using var reader = new StreamReader(stream);
                    var content = await reader.ReadToEndAsync();
                    drugs = JsonSerializer.Deserialize<List<DrugImportDto>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<DrugImportDto>();
                }
                else
                {
                    drugs = JsonSerializer.Deserialize<List<DrugImportDto>>(jsonText!,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<DrugImportDto>();
                }

                var result = await _apiService.PostAsync<ServiceResult>("api/Admin/import-drugs", drugs);
                if (result?.Success == true)
                {
                    TempData["SuccessMessage"] = result.Message;
                }
                else
                {
                    TempData["ErrorMessage"] = result?.Message ?? "فشل في استيراد الأدوية.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing drugs from AdminPortal");
                TempData["ErrorMessage"] = "حدث خطأ أثناء استيراد الأدوية: " + ex.Message;
            }

            return View();
        }

        // ==================== Bulk Import: Interactions ====================
        [HttpGet]
        public IActionResult ImportInteractions()
        {
            CheckAuthentication();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportInteractions(IFormFile? file, string? jsonText)
        {
            CheckAuthentication();

            if ((file == null || file.Length == 0) && string.IsNullOrWhiteSpace(jsonText))
            {
                TempData["ErrorMessage"] = "يرجى اختيار ملف JSON أو لصق محتوى JSON.";
                return View();
            }

            try
            {
                List<InteractionImportDto> interactions;

                if (file != null && file.Length > 0)
                {
                    using var stream = file.OpenReadStream();
                    using var reader = new StreamReader(stream);
                    var content = await reader.ReadToEndAsync();
                    interactions = JsonSerializer.Deserialize<List<InteractionImportDto>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<InteractionImportDto>();
                }
                else
                {
                    interactions = JsonSerializer.Deserialize<List<InteractionImportDto>>(jsonText!,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<InteractionImportDto>();
                }

                var result = await _apiService.PostAsync<ServiceResult>("api/Admin/import-interactions", interactions);
                if (result?.Success == true)
                {
                    TempData["SuccessMessage"] = result.Message;
                }
                else
                {
                    TempData["ErrorMessage"] = result?.Message ?? "فشل في استيراد التفاعلات.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing interactions from AdminPortal");
                TempData["ErrorMessage"] = "حدث خطأ أثناء استيراد التفاعلات: " + ex.Message;
            }

            return View();
        }

        // ==================== Reset User Password ====================
        [HttpGet]
        public IActionResult ResetUserPassword()
        {
            CheckAuthentication();
            return View(new ResetPasswordRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetUserPassword(ResetPasswordRequest model)
        {
            CheckAuthentication();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var response = await _apiService.PostAsync<ResetPasswordResponse>("api/Admin/reset-password", model);

                if (response?.Success == true)
                {
                    TempData["SuccessMessage"] = response.Message;
                    ViewBag.GeneratedPassword = response.GeneratedPassword;
                }
                else
                {
                    TempData["ErrorMessage"] = response?.Message ?? "فشل في إعادة تعيين كلمة المرور.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for user {UserId}", model.UserId);
                TempData["ErrorMessage"] = "حدث خطأ أثناء إعادة تعيين كلمة المرور: " + ex.Message;
            }

            return View(model);
        }
    }
}

