using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatientMedicalRecords.Data;
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

        public AttachmentsController(MedicalRecordsDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
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
