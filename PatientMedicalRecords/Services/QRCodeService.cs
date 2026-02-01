using Microsoft.EntityFrameworkCore;
using PatientMedicalRecords.Data;
using PatientMedicalRecords.DTOs;
using PatientMedicalRecords.Models;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using ZXing.QrCode.Internal;


namespace PatientMedicalRecords.Services
{
    public interface IQRCodeService
    {
        Task<QRCodeResponse> GenerateQRCodeAsync(QRCodeRequest request);
        Task<AccessResponse> ValidateAccessTokenAsync(AccessRequest request);
        Task<AccessResponse> RequestAccessAsync(string token, int requesterUserId);
        Task<ApprovalResponse> ApproveAccessAsync(ApprovalRequest request, int patientId);
        Task<bool> RevokeTokenAsync(string token);
    }

    public class QRCodeService : IQRCodeService
    {
        private readonly MedicalRecordsDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<QRCodeService> _logger;
        private readonly string _baseUrl;

        public QRCodeService(
            MedicalRecordsDbContext context,
            IConfiguration configuration,
            ILogger<QRCodeService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _baseUrl = _configuration["BaseUrl"] ?? "https://api.medrec.gov.sa";
        }

        public async Task<QRCodeResponse> GenerateQRCodeAsync(QRCodeRequest request)
        {
            try
            {
                // Check if user exists and is a patient (Updated 26-01-2026 to support multi-role)
                var user = await _context.Users
                    .Include(u => u.Patient)
                    .Include(u => u.Roles)
                    .FirstOrDefaultAsync(u => u.Id == request.UserId && (u.Role == UserRole.Patient || u.Roles.Any(r => r.Role == UserRole.Patient)));

                if (user == null || user.Patient == null)
                {
                    return new QRCodeResponse
                    {
                        Success = false,
                        Message = "المستخدم غير موجود أو ليس مريضاً"
                    };
                }

                // Generate unique token
                var token = GenerateUniqueToken();
                var expiresAt = DateTime.UtcNow.AddMinutes(5); // 5 minutes expiration

                // Create access token
                var accessToken = new AccessToken
                {
                    UserId = request.UserId,
                    Token = token,
                    Status = AccessTokenStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = expiresAt
                };

                _context.AccessTokens.Add(accessToken);
                await _context.SaveChangesAsync();

                // Generate QR Code
                var qrCodeUrl = await GenerateQRCodeImageAsync(token);
                var qrCodeDataUrl = $"data:image/png;base64,{qrCodeUrl}";

                // Log the action
                await LogUserAction(request.UserId, "GENERATE_QR", "تم توليد رمز QR جديد");

                return new QRCodeResponse
                {
                    Success = true,
                    Message = "تم توليد رمز QR بنجاح",
                    QRCodeUrl = qrCodeDataUrl,
                    Token = token,
                    ExpiresAt = expiresAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code for user {UserId}", request.UserId);
                return new QRCodeResponse
                {
                    Success = false,
                    Message = "حدث خطأ أثناء توليد رمز QR"
                };
            }
        }

        public async Task<AccessResponse> ValidateAccessTokenAsync(AccessRequest request)
        {
            try
            {
                var accessToken = await _context.AccessTokens
                    .Include(at => at.User)
                    .FirstOrDefaultAsync(at => at.Token == request.Token);

                if (accessToken == null)
                {
                    return new AccessResponse
                    {
                        Success = false,
                        Message = "الرمز غير صحيح"
                    };
                }

                // Check if token is expired
                if (accessToken.ExpiresAt < DateTime.UtcNow)
                {
                    accessToken.Status = AccessTokenStatus.Expired;
                    await _context.SaveChangesAsync();

                    return new AccessResponse
                    {
                        Success = false,
                        Message = "انتهت صلاحية الرمز"
                    };
                }

                // Check if token is already used
                if (accessToken.Status == AccessTokenStatus.Used)
                {
                    return new AccessResponse
                    {
                        Success = false,
                        Message = "تم استخدام هذا الرمز مسبقاً"
                    };
                }

                // Check if token is revoked
                if (accessToken.Status == AccessTokenStatus.Revoked)
                {
                    return new AccessResponse
                    {
                        Success = false,
                        Message = "تم إلغاء هذا الرمز"
                    };
                }

                return new AccessResponse
                {
                    Success = true,
                    Message = "الرمز صحيح",
                    RequiresApproval = true,
                    RequestId = accessToken.Id
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating access token {Token}", request.Token);
                return new AccessResponse
                {
                    Success = false,
                    Message = "حدث خطأ أثناء التحقق من الرمز"
                };
            }
        }

        public async Task<AccessResponse> RequestAccessAsync(string token, int requesterUserId)
        {
            try
            {
                var accessToken = await _context.AccessTokens
                    .Include(at => at.User)
                    .FirstOrDefaultAsync(at => at.Token == token);

                if (accessToken == null)
                {
                    return new AccessResponse
                    {
                        Success = false,
                        Message = "الرمز غير صحيح"
                    };
                }

                // Check if token is still valid
                if (accessToken.ExpiresAt < DateTime.UtcNow || accessToken.Status != AccessTokenStatus.Active)
                {
                    return new AccessResponse
                    {
                        Success = false,
                        Message = "الرمز غير صالح أو منتهي الصلاحية"
                    };
                }

                // Get requester info
                var requester = await _context.Users.FindAsync(requesterUserId);
                if (requester == null)
                {
                    return new AccessResponse
                    {
                        Success = false,
                        Message = "مقدم الطلب غير موجود"
                    };
                }

                // Log the access request
                await LogUserAction(requesterUserId, "REQUEST_ACCESS", 
                    $"طلب وصول للمريض {accessToken.User.FullName} (ID: {accessToken.UserId})");

                // For now, we'll mark the token as used immediately
                // In a real implementation, you might want to send a notification to the patient
                accessToken.Status = AccessTokenStatus.Used;
                accessToken.UsedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return new AccessResponse
                {
                    Success = true,
                    Message = "تم طلب الوصول بنجاح",
                    RequiresApproval = false,
                    RequestId = accessToken.Id
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting access with token {Token}", token);
                return new AccessResponse
                {
                    Success = false,
                    Message = "حدث خطأ أثناء طلب الوصول"
                };
            }
        }

        public async Task<ApprovalResponse> ApproveAccessAsync(ApprovalRequest request, int patientId)
        {
            try
            {
                var accessToken = await _context.AccessTokens
                    .Include(at => at.User)
                    .FirstOrDefaultAsync(at => at.Id == request.RequestId && at.UserId == patientId);

                if (accessToken == null)
                {
                    return new ApprovalResponse
                    {
                        Success = false,
                        Message = "طلب الوصول غير موجود"
                    };
                }

                if (request.Approved)
                {
                    accessToken.Status = AccessTokenStatus.Used;
                    accessToken.UsedAt = DateTime.UtcNow;
                    await LogUserAction(patientId, "APPROVE_ACCESS", "تم الموافقة على طلب الوصول");
                }
                else
                {
                    accessToken.Status = AccessTokenStatus.Revoked;
                    await LogUserAction(patientId, "REJECT_ACCESS", "تم رفض طلب الوصول");
                }

                await _context.SaveChangesAsync();

                return new ApprovalResponse
                {
                    Success = true,
                    Message = request.Approved ? "تم الموافقة على الوصول" : "تم رفض الوصول"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving access for request {RequestId}", request.RequestId);
                return new ApprovalResponse
                {
                    Success = false,
                    Message = "حدث خطأ أثناء معالجة طلب الوصول"
                };
            }
        }

        public async Task<bool> RevokeTokenAsync(string token)
        {
            try
            {
                var accessToken = await _context.AccessTokens
                    .FirstOrDefaultAsync(at => at.Token == token);

                if (accessToken == null) return false;

                accessToken.Status = AccessTokenStatus.Revoked;
                await _context.SaveChangesAsync();

                await LogUserAction(accessToken.UserId, "REVOKE_TOKEN", "تم إلغاء الرمز");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking token {Token}", token);
                return false;
            }
        }

        private string GenerateUniqueToken()
        {
            var random = new Random();
            var bytes = new byte[16];
            random.NextBytes(bytes);
            return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
        }

        //private async Task<string> GenerateQRCodeImageAsync(string token)
        //{
        //    try
        //    {
        //        var qrCodeUrl = $"{_baseUrl}/api/access?token={token}";

        //        using var qrGenerator = new QRCodeGenerator();
        //        using var qrCodeData = qrGenerator.CreateQrCode(qrCodeUrl, QRCodeGenerator.ECCLevel.Q);
        //        using var qrCode = new QRCode(qrCodeData);
        //        using var qrCodeImage = qrCode.GetGraphic(20);

        //        using var memoryStream = new MemoryStream();
        //        qrCodeImage.Save(memoryStream, ImageFormat.Png);
        //        var imageBytes = memoryStream.ToArray();

        //        return Convert.ToBase64String(imageBytes);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error generating QR code image for token {Token}", token);
        //        throw;
        //    }
        //}

        private async Task<string> GenerateQRCodeImageAsync(string token)
        {
            try
            {
                var qrCodeUrl = $"{_baseUrl}/api/access?token={token}";

                using var qrGenerator = new QRCodeGenerator();
                using var qrCodeData = qrGenerator.CreateQrCode(qrCodeUrl, QRCodeGenerator.ECCLevel.Q);

                var pngWriter = new PngByteQRCode(qrCodeData);
                byte[] pngBytes = pngWriter.GetGraphic(20); // يعيد مباشرة مصفوفة بايت لصورة PNG

                return Convert.ToBase64String(pngBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code image for token {Token}", token);
                throw;
            }
        }




















        private async Task LogUserAction(int userId, string action, string description)
        {
            try
            {
                _context.AuditLogs.Add(new AuditLog
                {
                    UserId = userId,
                    Action = action,
                    Description = description,
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging user action {Action} for user {UserId}", action, userId);
            }
        }
    }
}
