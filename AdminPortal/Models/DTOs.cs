using System.ComponentModel.DataAnnotations;

namespace AdminPortal.Models
{
    // Login DTOs
    public class LoginRequest
    {
        [Required]
        public string Identifier { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? AccessToken { get; set; }
        public string? Role { get; set; }
        public int? UserId { get; set; }
    }

    // User Management DTOs
    public class UserListRequest
    {
        public UserRole? Role { get; set; }
        public UserStatus? Status { get; set; }
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
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

    public class UserInfo
    {
        public int Id { get; set; }
        public string NationalId { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public UserStatus Status { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }

    public class RegistrationDecisionRequest
    {
        [Required]
        public bool IsApproved { get; set; }
        public string? Notes { get; set; }
    }

    public class UserStatusUpdateRequest
    {
        [Required]
        public int UserId { get; set; }
        [Required]
        public UserStatus Status { get; set; }
        public string? Reason { get; set; }
    }

    public class UserStatusUpdateResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    // Pending Registration DTOs
    public class PendingRegistration
    {
        public int Id { get; set; }
        public string NationalId { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public UserStatus Status { get; set; }
        public DoctorInfo? DoctorInfo { get; set; }
        public PharmacistInfo? PharmacistInfo { get; set; }
        public List<AttachmentInfo> Attachments { get; set; } = new List<AttachmentInfo>();
    }

    public class DoctorInfo
    {
        public string FullName { get; set; } = string.Empty;
        public string? Specialization { get; set; }
        public string LicenseNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Hospital { get; set; }
    }

    public class PharmacistInfo
    {
        public string FullName { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? PharmacyName { get; set; }
    }

    public class AttachmentInfo
    {
        public int Id { get; set; }
        public string AttachmentType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime CreatedAt { get; set; }
        public string DownloadUrl { get; set; } = string.Empty;
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
        public List<AuditLogInfo> Logs { get; set; } = new List<AuditLogInfo>();
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

    // Notification DTOs
    public class NotificationRequest
    {
        [Required]
        public List<int> UserIds { get; set; } = new List<int>();
        [Required]
        public string Subject { get; set; } = string.Empty;
        [Required]
        public string Content { get; set; } = string.Empty;
    }

    public class NotificationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int SentCount { get; set; }
        public int FailedCount { get; set; }
    }
}

