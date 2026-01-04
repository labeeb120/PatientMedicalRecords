using System.ComponentModel.DataAnnotations;


namespace PatientMedicalRecords.Models
{
    public class User
    {
        [Key] 
        public int Id { get; set; }
        [Required][StringLength(10, MinimumLength = 9)]
        [RegularExpression(@"^\d{9,15}$", ErrorMessage = "الرقم الوطني يجب أن يتكون من 9 أو 15 أرقام فقط")]
        public string NationalId { get; set; } = string.Empty;

        [Required][StringLength(255)] 
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [EnumDataType(typeof(UserStatus))]
        public UserRole Role { get; set; }
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

    }
}
