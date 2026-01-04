using System.ComponentModel.DataAnnotations;
using PatientMedicalRecords.Models;

namespace PatientMedicalRecords.DTOs
{
    // Login DTOs
    //public class LoginRequest
    //{
    //    [Required(ErrorMessage = "الرقم الوطني مطلوب")]
    //    [StringLength(10, MinimumLength = 9, ErrorMessage = "الرقم الوطني يجب أن يكون بين 9-10 أرقام")]
    //    public string NationalId { get; set; } = string.Empty;

    //    [Required(ErrorMessage = "كلمة المرور مطلوبة")]
    //    [StringLength(100, MinimumLength = 6, ErrorMessage = "كلمة المرور يجب أن تكون على الأقل 6 أحرف")]
    //    public string Password { get; set; } = string.Empty;

    //    [Required(ErrorMessage = "الدور مطلوب")]
    //    public UserRole Role { get; set; }
    //}


    public class LoginRequest
    {
        public string Identifier { get; set; } = string.Empty; // يمكن أن يكون PatientCode أو NationalId
        public string Password { get; set; } = string.Empty;
        public string? DeviceToken { get; set; } // اختياري: للاشعارات
        public string? DevicePlatform { get; set; } // Android/iOS/Web
    }



    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
        public string? Role { get; set; }
        public int? UserId { get; set; }
        public object? User { get; set; } // اختياري الحقول التي تريد إرسالها
    }

    //public class LoginResponse
    //{
    //    public bool Success { get; set; }
    //    public string Message { get; set; } = string.Empty;
    //    public string Token { get; set; } = string.Empty;
    //    public DateTime ExpiresAt { get; set; }
    //    public UserInfo? User { get; set; }
    //}

    // Register DTOs
    public class RegisterRequest
    {
        [Required(ErrorMessage = "الرقم الوطني مطلوب")]
        [StringLength(15, MinimumLength = 9, ErrorMessage = "الرقم الوطني يجب أن يكون بين 9-15 أرقام")]
        public string NationalId { get; set; } = string.Empty;

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "كلمة المرور يجب أن تكون على الأقل 6 أحرف")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "تأكيد كلمة المرور مطلوب")]
        [Compare("Password", ErrorMessage = "كلمة المرور وتأكيد كلمة المرور غير متطابقين")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "الدور مطلوب")]
        public UserRole Role { get; set; }
    }

    public class RegisterResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int UserId { get; set; }
    }

    // User Info DTO
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

    // Change Password DTO
    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "كلمة المرور الحالية مطلوبة")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "كلمة المرور الجديدة مطلوبة")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "كلمة المرور يجب أن تكون على الأقل 6 أحرف")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "تأكيد كلمة المرور مطلوب")]
        [Compare("NewPassword", ErrorMessage = "كلمة المرور وتأكيد كلمة المرور غير متطابقين")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    public class ChangePasswordResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

    }

    // QR Code DTOs
    public class QRCodeRequest
    {
        [Required(ErrorMessage = "معرف المستخدم مطلوب")]
        public int UserId { get; set; }
    }

    public class QRCodeResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string QRCodeUrl { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }

    // Access Request DTOs
    public class AccessRequest
    {
        [Required(ErrorMessage = "الرمز مطلوب")]
        public string Token { get; set; } = string.Empty;
    }

    public class AccessResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool RequiresApproval { get; set; }
        public int RequestId { get; set; }
    }

    // Approval DTOs
    public class ApprovalRequest
    {
        [Required(ErrorMessage = "معرف الطلب مطلوب")]
        public int RequestId { get; set; }

        [Required(ErrorMessage = "القرار مطلوب")]
        public bool Approved { get; set; }

        public string? Notes { get; set; }
    }


    public class ApproveUserRequest
    {
        [Required(ErrorMessage = "معرف الطلب مطلوب")]
        public int RequestId { get; set; }

        [Required(ErrorMessage = "القرار مطلوب")]
        public bool Approved { get; set; }

        public string? Notes { get; set; }
    }

    public class ApprovalResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }


    public class PatientRegisterRequest
    {
        public string NationalId { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
    }

    public class DoctorRegisterRequest
    {
        public string NationalId { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;        
        public string? Specialization { get; set; }
        public string? Hospital { get; set; }
        [Required(ErrorMessage = "شهادة الترخيص مطلوبة")]
        public IFormFile LicenseDocument { get; set; }
    }

    public class PharmacistRegisterRequest
    {
        public string NationalId { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public string? PharmacyName { get; set; }    
        [Required(ErrorMessage = "شهادة الترخيص مطلوبة")]
        public IFormFile LicenseDocument { get; set; }

    }





}
