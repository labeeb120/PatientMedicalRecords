using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PatientMedicalRecords.Models
{
    public class AuditLog
    {
        [Key] public int Id { get; set; }
        [Required] public int UserId { get; set; }
        [Required][StringLength(100)] public string Action { get; set; } = string.Empty;
        [StringLength(200)] public string? Description { get; set; }
        [StringLength(50)] public string? IpAddress { get; set; }
        [StringLength(200)] public string? UserAgent { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")] public virtual User User { get; set; } = null!;
    }
}
