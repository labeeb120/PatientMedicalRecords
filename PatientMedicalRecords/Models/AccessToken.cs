using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PatientMedicalRecords.Models
{
    public class AccessToken
    {
        [Key] public int Id { get; set; }
        [Required] public int UserId { get; set; }
        [Required][StringLength(100)] public string Token { get; set; } = string.Empty;
        [Required] public AccessTokenStatus Status { get; set; } = AccessTokenStatus.Active;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public DateTime? UsedAt { get; set; }

        [ForeignKey("UserId")] public virtual User User { get; set; } = null!;
    }
}
