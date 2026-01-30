using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatientMedicalRecords.Data;
using PatientMedicalRecords.DTOs;
using PatientMedicalRecords.Models;
using SkiaSharp;
using System.Linq;
using System.Security.Claims;

namespace PatientMedicalRecords.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AdminController : ControllerBase
    {
        private readonly MedicalRecordsDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(MedicalRecordsDbContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        ///****************************************************************
        /// Get all pending doctor + pharmacist accounts
        /// <summary>
        /// عرض طلبات الموافقة على التسجيل
        /// </summary>       
        /// 

        // In AdminController.cs

        [HttpGet("pending-registrations")]
        public async Task<IActionResult> GetPendingRegistrations()
        {
            var pendingUsers = await _context.Users
                .Where(u => u.Status == UserStatus.Pending)
                .Include(u => u.Attachments) // <-- تضمين المرفقات
                .Select(u => new
                {
                    u.Id,
                    u.NationalId,
                    u.Role,
                    u.Status,

                    // معلومات الطبيب
                    DoctorInfo = u.Role == UserRole.Doctor
                        ? _context.Doctors
                            .Where(d => d.UserId == u.Id)
                            .Select(d => new { d.FullName, d.Specialization, d.LicenseNumber, d.Email, d.PhoneNumber, d.Hospital })
                            .FirstOrDefault()
                        : null,

                    // معلومات الصيدلي
                    PharmacistInfo = u.Role == UserRole.Pharmacist
                        ? _context.Pharmacists
                            .Where(p => p.UserId == u.Id)
                            .Select(p => new { p.FullName, p.LicenseNumber, p.Email, p.PhoneNumber, p.PharmacyName })
                            .FirstOrDefault()
                        : null,

                    // قائمة المرفقات
                    Attachments = u.Attachments.Select(a => new
                    {
                        a.Id,
                        a.AttachmentType,
                        a.FileName,
                        a.FileSize,
                        a.CreatedAt,
                        // سنقوم بإنشاء رابط لتنزيل الملف لاحقاً
                        DownloadUrl = $"/api/Attachments/download/{a.Id}"
                    }).ToList()

                })
                .ToListAsync();

            return Ok(pendingUsers);
        }

        //*******************************************************************************
        [HttpGet("pending-registrations/{userId}")]
        public async Task<IActionResult> GetPendingRegistrationDetails(int userId)
        {
            var user = await _context.Users
                .Where(u => u.Id == userId && u.Status == UserStatus.Pending)
                // ... (نفس منطق جلب البيانات الموجود في GetPendingRegistrations)
                .FirstOrDefaultAsync();

            if (user == null) return NotFound();

            // ... (قم بإرجاع بيانات المستخدم بنفس شكل PendingRegistrationViewModel)
            return Ok(user);
        }

        //********************************************************************************

        [HttpPost("registrations/{userId}/decision")]
        public async Task<IActionResult> DecideOnRegistration(int userId, [FromBody] RegistrationDecisionRequest request)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("User not found");

            user.Status = request.IsApproved ? UserStatus.Approved : UserStatus.Rejected;
            // يمكنك هنا إضافة ملاحظات الرفض أو القبول
            await _context.SaveChangesAsync();
            return Ok($"User registration has been {(request.IsApproved ? "approved" : "rejected")}.");
        }



        /// <summary>
        /// الموافقة على طلب تسجيل المستخدم
        /// </summary>
        [HttpPost("approve/{userId}")]
        public async Task<IActionResult> ApproveRegistration(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("User not found");

            user.Status = UserStatus.Approved;

            await _context.SaveChangesAsync();

            return Ok("User approved successfully");
        }


        /// <summary>
        /// رفض طلب التسجيل
        /// </summary>
        [HttpPost("reject/{userId}")]
        public async Task<IActionResult> RejectRegistration(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("User not found");

            user.Status = UserStatus.Rejected;

            await _context.SaveChangesAsync();

            return Ok("User registration rejected");
        }

        //***************************************************************
        /// <summary>
        /// الحصول على قائمة المستخدمين
        /// </summary>
        [HttpPost("GetUsers")]
        public async Task<ActionResult<UserListResponse>> GetUsers([FromBody] UserListRequest request)
        {
            try
            {
                if (!IsAdmin()) return Forbid();

                var query = _context.Users.AsQueryable();

                // Filter by role (26-01-2026: Updated to support multi-role via UserRoleAssignments)
                if (request.Role.HasValue)
                {
                    query = query.Where(u => u.Roles.Any(r => r.Role == request.Role.Value) || u.Role == request.Role.Value);
                }

                // Filter by status
                if (request.Status.HasValue)
                {
                    query = query.Where(u => u.Status == request.Status.Value);
                }

                // Search by name or national ID
                //if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                //{
                //    query = query.Where(u => u.FullName.Contains(request.SearchTerm) || 
                //                           u.NationalId.Contains(request.SearchTerm));
                //}

                var totalCount = await query.CountAsync();

                var users = await query
                    .OrderByDescending(u => u.CreatedAt)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)                   
                    .Select(u => new UserInfo
                    {
                        Id = u.Id,
                        NationalId = u.NationalId,
                        Role = u.Role,
                        Status = u.Status,
                        FullName = u.FullName,
                        PhoneNumber = u.PhoneNumber,
                        Email = u.Email,
                        CreatedAt = u.CreatedAt,
                        LastLoginAt = u.LastLoginAt
                    })
                    .ToListAsync();

                var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

                return Ok(new UserListResponse
                {
                    Success = true,
                    Message = "تم جلب قائمة المستخدمين بنجاح",
                    Users = users,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalPages = totalPages
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users list");
                return StatusCode(500, new UserListResponse
                {
                    Success = false,
                    Message = "حدث خطأ في الخادم"
                });
            }
        }

        

        /// <summary>
        /// تحديث حالة المستخدم
        /// </summary>
        [HttpPut("user-status")]
        public async Task<ActionResult<UserStatusUpdateResponse>> UpdateUserStatus([FromBody] UserStatusUpdateRequest request)
        {
            try
            {
                if (!IsAdmin()) return Forbid();

                var user = await _context.Users.FindAsync(request.UserId);
                if (user == null)
                {
                    return NotFound(new UserStatusUpdateResponse
                    {
                        Success = false,
                        Message = "المستخدم غير موجود"
                    });
                }

                user.Status = request.Status;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Log the action
                await LogUserAction(GetCurrentUserId()!.Value, "UPDATE_USER_STATUS", 
                    $"تم تحديث حالة المستخدم {user.NationalId} إلى {request.Status}");

                return Ok(new UserStatusUpdateResponse
                {
                    Success = true,
                    Message = "تم تحديث حالة المستخدم بنجاح"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user status {UserId}", request.UserId);
                return StatusCode(500, new UserStatusUpdateResponse
                {
                    Success = false,
                    Message = "حدث خطأ في الخادم"
                });
            }
        }

        /// <summary>
        /// الحصول على سجل التدقيق
        /// </summary>
        [HttpPost("audit-logs")]
        public async Task<ActionResult<AuditLogResponse>> GetAuditLogs([FromBody] AuditLogRequest request)
        {
            try
            {
                if (!IsAdmin()) return Forbid();

                var query = _context.AuditLogs
                    .Include(al => al.User)
                    .AsQueryable();

                // Filter by user
                if (request.UserId.HasValue)
                {
                    query = query.Where(al => al.UserId == request.UserId.Value);
                }

                // Filter by action
                //if (!string.IsNullOrWhiteSpace(request.Action))
                //{
                //    query = query.Where(al => al.Action.Contains(request.Action));
                //}

                // Filter by date range
                if (request.StartDate.HasValue)
                {
                    query = query.Where(al => al.CreatedAt >= request.StartDate.Value);
                }

                if (request.EndDate.HasValue)
                {
                    query = query.Where(al => al.CreatedAt <= request.EndDate.Value);
                }

                var totalCount = await query.CountAsync();

                var logs = await query
                    .OrderByDescending(al => al.CreatedAt)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(al => new AuditLogInfo
                    {
                        Id = al.Id,
                        UserId = al.UserId,
                        UserName = al.User.FullName ?? al.User.NationalId,
                        Action = al.Action,
                        Description = al.Description,
                        IpAddress = al.IpAddress,
                        UserAgent = al.UserAgent,
                        CreatedAt = al.CreatedAt
                    })
                    .ToListAsync();

                var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

                return Ok(new AuditLogResponse
                {
                    Success = true,
                    Message = "تم جلب سجل التدقيق بنجاح",
                    Logs = logs,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalPages = totalPages
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting audit logs");
                return StatusCode(500, new AuditLogResponse
                {
                    Success = false,
                    Message = "حدث خطأ في الخادم"
                });
            }
        }

        /// <summary>
        /// الحصول على إحصائيات النظام
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult<StatisticsResponse>> GetStatistics()
        {
            try
            {
                if (!IsAdmin()) return Forbid();

                var statistics = new SystemStatistics
                {
                    TotalUsers = await _context.Users.CountAsync(),
                    TotalPatients = await _context.Patients.CountAsync(),
                    TotalDoctors = await _context.Doctors.CountAsync(),
                    TotalPharmacists = await _context.Pharmacists.CountAsync(),
                    TotalMedicalRecords = await _context.MedicalRecords.CountAsync(),
                    TotalPrescriptions = await _context.Prescriptions.CountAsync(),
                    PendingApprovals = await _context.Users.CountAsync(u => u.Status == UserStatus.Pending),
                    ActiveUsers = await _context.Users.CountAsync(u => u.Status == UserStatus.Approved),
                    SuspendedUsers = await _context.Users.CountAsync(u => u.Status == UserStatus.Suspended)
                };

                // Monthly statistics for the last 12 months
                var startDate = DateTime.UtcNow.AddMonths(-12);
                var monthlyStats = await _context.Users
                    //.Where(u => u.CreatedAt != startDate)
                    .GroupBy(u => new { u.CreatedAt.Year, u.CreatedAt.Month })
                    .Select(g => new MonthlyStatistics
                    {
                        Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                        NewUsers = g.Count()
                    })
                    //.OrderBy(ms => ms.Month)
                    .ToListAsync();

                statistics.MonthlyStats = monthlyStats;

                // Role statistics (26-01-2026: Re-implemented to support multi-role counting)
                var roles = Enum.GetValues(typeof(UserRole)).Cast<UserRole>();
                var roleStats = new List<RoleStatistics>();

                foreach (var r in roles)
                {
                    var count = await _context.Users.CountAsync(u => u.Roles.Any(ra => ra.Role == r) || u.Role == r);
                    if (count > 0)
                    {
                        roleStats.Add(new RoleStatistics
                        {
                            Role = r,
                            RoleName = r.ToString(),
                            Count = count,
                            ActiveCount = await _context.Users.CountAsync(u => (u.Roles.Any(ra => ra.Role == r) || u.Role == r) && u.Status == UserStatus.Approved),
                            PendingCount = await _context.Users.CountAsync(u => (u.Roles.Any(ra => ra.Role == r) || u.Role == r) && u.Status == UserStatus.Pending),
                            SuspendedCount = await _context.Users.CountAsync(u => (u.Roles.Any(ra => ra.Role == r) || u.Role == r) && u.Status == UserStatus.Suspended)
                        });
                    }
                }

                statistics.RoleStats = roleStats;

                return Ok(new StatisticsResponse
                {
                    Success = true,
                    Message = "تم جلب إحصائيات النظام بنجاح",
                    Statistics = statistics
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system statistics");
                return StatusCode(500, new StatisticsResponse
                {
                    Success = false,
                    Message = "حدث خطأ في الخادم"
                });
            }
        }

        /// <summary>
        /// إرسال إشعار للمستخدمين
        /// </summary>
        [HttpPost("send-notification")]
        public async Task<ActionResult<NotificationResponse>> SendNotification([FromBody] NotificationRequest request)
        {
            try
            {
                if (!IsAdmin()) return Forbid();

                // In a real implementation, you would integrate with email/SMS services
                // For now, we'll just log the notification

                var sentCount = 0;
                var failedCount = 0;

                foreach (var userId in request.UserIds)
                {
                    try
                    {
                        // Log the notification
                        await LogUserAction(userId, "NOTIFICATION_RECEIVED", 
                            $"إشعار: {request.Subject} - {request.Content}");
                        sentCount++;
                    }
                    catch
                    {
                        failedCount++;
                    }
                }

                return Ok(new NotificationResponse
                {
                    Success = true,
                    Message = "تم إرسال الإشعارات بنجاح",
                    SentCount = sentCount,
                    FailedCount = failedCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification");
                return StatusCode(500, new NotificationResponse
                {
                    Success = false,
                    Message = "حدث خطأ في الخادم"
                });
            }
        }

        private bool IsAdmin()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role);
            return roleClaim?.Value == UserRole.Admin.ToString();
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return null;
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






///// <summary>
///// الموافقة على طلب تسجيل المستخدم
///// </summary>
//[HttpPost("approve-user")]
//public async Task<ActionResult<UserApprovalResponse>> ApproveUser([FromBody] UserApprovalRequest request)
//{
//    try
//    {
//        if (!IsAdmin()) return Forbid();

//        var user = await _context.Users.FindAsync(request.UserId);
//        if (user == null)
//        {
//            return NotFound(new UserApprovalResponse
//            {
//                Success = false,
//                Message = "المستخدم غير موجود"
//            });
//        }

//        user.Status = request.Approved ? UserStatus.Approved : UserStatus.Rejected;
//        user.UpdatedAt = DateTime.UtcNow;

//        await _context.SaveChangesAsync();

//        // Log the action
//        await LogUserAction(GetCurrentUserId()!.Value, "APPROVE_USER",
//            $"تم {(request.Approved ? "الموافقة على" : "رفض")} طلب تسجيل المستخدم {user.NationalId}");

//        return Ok(new UserApprovalResponse
//        {
//            Success = true,
//            Message = request.Approved ? "تم الموافقة على المستخدم" : "تم رفض المستخدم"
//        });
//    }
//    catch (Exception ex)
//    {
//        _logger.LogError(ex, "Error approving user {UserId}", request.UserId);
//        return StatusCode(500, new UserApprovalResponse
//        {
//            Success = false,
//            Message = "حدث خطأ في الخادم"
//        });
//    }
//}




//[HttpGet("pending-registrations")]
//public async Task<IActionResult> GetPendingRegistrations()
//{
//    var pendingUsers = await _context.Users
//        .Where(u => u.Status == UserStatus.Pending)
//        .Select(u => new
//        {
//            u.Id,
//            u.NationalId,
//            u.Role,
//            u.Status,

//            // معلومات الطبيب (فقط إذا كان دوره Doctor)
//            DoctorInfo = u.Role == UserRole.Doctor
//                ? _context.Doctors
//                    .Where(d => d.UserId == u.Id)
//                    .Select(d => new {
//                        d.FullName,
//                        d.Specialization,
//                        d.LicenseNumber,
//                        d.Email,
//                        d.PhoneNumber,
//                        d.Hospital
//                    })
//                    .FirstOrDefault()
//                : null,

//            // معلومات الصيدلي (فقط إذا كان دوره Pharmacist)
//            PharmacistInfo = u.Role == UserRole.Pharmacist
//                ? _context.Pharmacists
//                    .Where(p => p.UserId == u.Id)
//                    .Select(p => new {
//                        p.FullName,
//                        p.LicenseNumber,
//                        p.Email,
//                        p.PhoneNumber,
//                        p.PharmacyName
//                    })
//                    .FirstOrDefault()
//                : null
//        })
//        .ToListAsync();

//    return Ok(pendingUsers);
//}