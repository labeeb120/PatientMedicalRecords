using AdminPortal.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdminPortal.Controllers
{
    public class AttachmentsController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<AttachmentsController> _logger;

        public AttachmentsController(IApiService apiService, ILogger<AttachmentsController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        private bool IsAuthenticated()
        {
            return Request.Cookies.ContainsKey("AuthToken") || 
                   !string.IsNullOrEmpty(HttpContext.Session.GetString("AuthToken"));
        }

        public async Task<IActionResult> Download(int attachmentId)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var stream = await _apiService.GetStreamAsync($"api/Attachments/download/{attachmentId}");
                
                // Get file name from Content-Disposition header or use a default
                var fileName = $"attachment_{attachmentId}";
                
                return File(stream, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading attachment {AttachmentId}", attachmentId);
                TempData["ErrorMessage"] = "Failed to download attachment";
                return RedirectToAction("PendingRegistrations", "Admin");
            }
        }
    }
}

