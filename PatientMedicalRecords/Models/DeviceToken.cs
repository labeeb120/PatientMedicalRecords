using System.ComponentModel.DataAnnotations;

namespace PatientMedicalRecords.Models
{
    public class DeviceToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public string Token { get; set; } = string.Empty; // FCM token

        public string? Platform { get; set; } // Android/iOS/Web

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastSeenAt { get; set; }
    }
}
