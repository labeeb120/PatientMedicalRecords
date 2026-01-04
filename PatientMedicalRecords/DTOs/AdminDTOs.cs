using System.ComponentModel.DataAnnotations;
using PatientMedicalRecords.Models;

namespace PatientMedicalRecords.DTOs
{
    // User Management DTOs
    public class UserListRequest
    {
        public UserRole? Role { get; set; }
        public UserStatus? Status { get; set; }
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "CreatedAt"; // e.g., "FullName", "CreatedAt"
        public string SortDirection { get; set; } = "Descending"; // "Ascending" or "Descending"

    }

    public class UserListResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<UserInfo> Users { get; set; } = new List<UserInfo>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class UserApprovalRequest
    {
        [Required(ErrorMessage = "معرف المستخدم مطلوب")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "القرار مطلوب")]
        public bool Approved { get; set; }

        [StringLength(200, ErrorMessage = "الملاحظات لا يجب أن تتجاوز 200 حرف")]
        public string? Notes { get; set; }
    }

    public class UserApprovalResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }


    public class RegistrationDecisionRequest
    {
        [Required]
        public bool IsApproved { get; set; }
        public string? Notes { get; set; }
    }


    public class UserStatusUpdateRequest
    {
        [Required(ErrorMessage = "معرف المستخدم مطلوب")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "الحالة الجديدة مطلوبة")]
        public UserStatus Status { get; set; }

        [StringLength(200, ErrorMessage = "السبب لا يجب أن يتجاوز 200 حرف")]
        public string? Reason { get; set; }
    }

    public class UserStatusUpdateResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    // Audit Log DTOs
    public class AuditLogRequest
    {
        public int? UserId { get; set; }
        public string? Action { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class AuditLogResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<AuditLogInfo> Logs { get; set; } //= new List<AuditLogInfo>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class AuditLogInfo
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Statistics DTOs
    public class StatisticsResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public SystemStatistics? Statistics { get; set; }
    }

    public class SystemStatistics
    {
        public int TotalUsers { get; set; }
        public int TotalPatients { get; set; }
        public int TotalDoctors { get; set; }
        public int TotalPharmacists { get; set; }
        public int TotalMedicalRecords { get; set; }
        public int TotalPrescriptions { get; set; }
        public int PendingApprovals { get; set; }
        public int ActiveUsers { get; set; }
        public int SuspendedUsers { get; set; }
        public List<MonthlyStatistics> MonthlyStats { get; set; } = new List<MonthlyStatistics>();
        public List<RoleStatistics> RoleStats { get; set; } = new List<RoleStatistics>();
    }

    public class MonthlyStatistics
    {
        public string Month { get; set; } = string.Empty;
        public int NewUsers { get; set; }
        public int NewMedicalRecords { get; set; }
        public int NewPrescriptions { get; set; }
    }

    public class RoleStatistics
    {
        public UserRole Role { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public int Count { get; set; }
        public int ActiveCount { get; set; }
        public int PendingCount { get; set; }
        public int SuspendedCount { get; set; }
    }

    // Backup DTOs
    public class BackupRequest
    {
        [StringLength(100, ErrorMessage = "اسم النسخة الاحتياطية لا يجب أن يتجاوز 100 حرف")]
        public string? BackupName { get; set; }
        public bool IncludeData { get; set; } = true;
        public bool IncludeLogs { get; set; } = false;
    }

    public class BackupResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? BackupPath { get; set; }
        public long? FileSize { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class BackupListResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<BackupInfo> Backups { get; set; } = new List<BackupInfo>();
    }

    public class BackupInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsValid { get; set; }
    }

    // System Settings DTOs
    public class SystemSettingsRequest
    {
        [Range(1, 60, ErrorMessage = "مدة صلاحية الرمز يجب أن تكون بين 1 و 60 دقيقة")]
        public int? TokenExpirationMinutes { get; set; }

        [Range(1, 100, ErrorMessage = "عدد المحاولات المسموحة يجب أن يكون بين 1 و 100")]
        public int? MaxLoginAttempts { get; set; }

        [Range(1, 1440, ErrorMessage = "مدة القفل يجب أن تكون بين 1 و 1440 دقيقة")]
        public int? LockoutDurationMinutes { get; set; }

        [Range(1, 100, ErrorMessage = "حجم الصفحة يجب أن يكون بين 1 و 100")]
        public int? DefaultPageSize { get; set; }

        public bool? EnableAuditLogging { get; set; }
        public bool? EnableEmailNotifications { get; set; }
        public bool? EnableSMSNotifications { get; set; }
    }

    public class SystemSettingsResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public SystemSettings? Settings { get; set; }
    }

    public class SystemSettings
    {
        public int TokenExpirationMinutes { get; set; } = 5;
        public int MaxLoginAttempts { get; set; } = 5;
        public int LockoutDurationMinutes { get; set; } = 15;
        public int DefaultPageSize { get; set; } = 10;
        public bool EnableAuditLogging { get; set; } = true;
        public bool EnableEmailNotifications { get; set; } = true;
        public bool EnableSMSNotifications { get; set; } = false;
        public DateTime LastUpdated { get; set; }
    }

    // Notification DTOs
    public class NotificationRequest
    {
        [Required(ErrorMessage = "المستلمون مطلوبون")]
        public List<int> UserIds { get; set; } = new List<int>();

        [Required(ErrorMessage = "الموضوع مطلوب")]
        [StringLength(200, ErrorMessage = "الموضوع لا يجب أن يتجاوز 200 حرف")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "المحتوى مطلوب")]
        [StringLength(1000, ErrorMessage = "المحتوى لا يجب أن يتجاوز 1000 حرف")]
        public string Content { get; set; } = string.Empty;

        public bool SendEmail { get; set; } = true;
        public bool SendSMS { get; set; } = false;
        public bool SendInApp { get; set; } = true;
    }

    public class NotificationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int SentCount { get; set; }
        public int FailedCount { get; set; }
    }
}
