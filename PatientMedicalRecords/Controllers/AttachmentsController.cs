using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatientMedicalRecords.Data;
using PatientMedicalRecords.Models;
using System.IO;
using System.Security.Claims;

namespace PatientMedicalRecords.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // يجب أن يكون المستخدم مسجلاً للدخول
    public class AttachmentsController : ControllerBase
    {
        private readonly MedicalRecordsDbContext _context;
        private readonly IWebHostEnvironment _env; // للوصول إلى مسار الملفات

        private readonly ILogger<AttachmentsController> _logger;

        public AttachmentsController(MedicalRecordsDbContext context, IWebHostEnvironment env, ILogger<AttachmentsController> logger)
        {
            _context = context;
            _env = env;
            _logger = logger;
        }

        /// <summary>
        /// رفع ملف (صورة أو PDF) وحفظه في المجلد المخصص
        /// </summary>
        [HttpPost("upload")]
        public async Task<IActionResult> UploadAttachment([FromForm] IFormFile file, [FromForm] string attachmentType)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { success = false, message = "لم يتم اختيار ملف" });

                // التحقق من نوع الملف (PDF أو صور فقط)
                var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                    return BadRequest(new { success = false, message = "نوع الملف غير مدعوم. المسموح به: PDF, JPG, PNG" });

                // التحقق من حجم الملف (أقصى حد 5 ميجابايت)
                if (file.Length > 5 * 1024 * 1024)
                    return BadRequest(new { success = false, message = "حجم الملف كبير جداً. الحد الأقصى 5 ميجابايت" });

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                    return Unauthorized(new { success = false, message = "غير مصرح لك" });

                // إنشاء المجلد إذا لم يكن موجوداً
                var uploadsFolder = Path.Combine(_env.ContentRootPath, "Attachments");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // إنشاء اسم فريد للملف لتجنب التكرار
                var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // حفظ البيانات في قاعدة البيانات
                var attachment = new UserAttachment
                {
                    UserId = userId,
                    AttachmentType = attachmentType ?? "General",
                    FileName = file.FileName,
                    FilePath = Path.Combine("Attachments", uniqueFileName),
                    ContentType = file.ContentType,
                    FileSize = file.Length,
                    CreatedAt = DateTime.UtcNow
                };

                _context.UserAttachments.Add(attachment);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "تم رفع الملف بنجاح",
                    attachmentId = attachment.Id,
                    fileName = attachment.FileName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                return StatusCode(500, new { success = false, message = "حدث خطأ في الخادم أثناء رفع الملف" });
            }
        }

        [HttpGet("download/{attachmentId}")]
        public async Task<IActionResult> DownloadAttachment(int attachmentId)
        {
            // فقط الأدمن يمكنه تنزيل أي ملف
            var isAdmin = User.FindFirst(ClaimTypes.Role)?.Value == "Admin";
            if (!isAdmin)
            {
                return Forbid("You do not have permission to download this file.");
            }

            var attachment = await _context.UserAttachments.FindAsync(attachmentId);

            if (attachment == null)
            {
                return NotFound("Attachment not found.");
            }

            // تأكد من أن مسار الملف آمن ولا يسمح بالوصول إلى ملفات خارج المجلد المخصص
            var fullPath = Path.Combine(_env.ContentRootPath, attachment.FilePath);

            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound("File does not exist on the server.");
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(fullPath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            // إرجاع الملف مع نوع المحتوى الصحيح
            return File(memory, attachment.ContentType, attachment.FileName);
        }
    }
}
