using System.ComponentModel.DataAnnotations;


namespace PatientMedicalRecords.Models
{
    public class User
    {
        [Key] 
        public int Id { get; set; }
        [Required][StringLength(15, MinimumLength = 9)]
        [RegularExpression(@"^\d{9,15}$", ErrorMessage = "الرقم الوطني يجب أن يتكون من 9 أو 15 أرقام فقط")]
        public string NationalId { get; set; } = string.Empty;

        [Required][StringLength(255)] 
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        //26-01-2026 تم الغاء هذا الحقل لانه سيتم التعامل مع الصلاحيات من خلال جدول وسيط بين المستخدم والصلاحيات
        [EnumDataType(typeof(UserStatus))]
        public UserRole Role { get; set; }
        public virtual ICollection<UserRoleAssignment> Roles { get; set; } = new List<UserRoleAssignment>();
        //public virtual ICollection<User> Users { get; set; }

        [Required]
        [EnumDataType(typeof(UserStatus))]
        public UserStatus Status { get; set; } = UserStatus.Pending;
        [StringLength(100)] 
        public string? FullName { get; set; }
        [StringLength(20)]
        [Phone(ErrorMessage = "صيغة رقم الهاتف غير صحيحة")]
        public string? PhoneNumber { get; set; }
        [StringLength(100)]
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        public virtual Patient? Patient { get; set; }
        public virtual Doctor? Doctor { get; set; }
        public virtual Pharmacist? Pharmacist { get; set; }
        public virtual ICollection<AccessToken> AccessTokens { get; set; } = new List<AccessToken>();
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
        // In User.cs
        public virtual ICollection<UserAttachment> Attachments { get; set; } = new List<UserAttachment>();
        //26-01-2026تم اضافة هذا السطر لدعم علاقة العديد إلى العديد بين المستخدم والصلاحيات
        // [ADD THIS LINE to User.cs]
        //public virtual ICollection<UserRoleAssignment> Roles { get; set; } = new List<UserRoleAssignment>();




    }
}
