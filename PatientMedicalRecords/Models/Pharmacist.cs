using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PatientMedicalRecords.Models
{
    public class Pharmacist
    {
        [Key] 
        [Required] public int UserId { get; set; }
        [Required][StringLength(100)] public string FullName { get; set; } = string.Empty;
        [StringLength(20)] public string? LicenseNumber { get; set; }
        [StringLength(100)] public string? PharmacyName { get; set; }
        [StringLength(20)] public string? PhoneNumber { get; set; }
        [StringLength(100)] public string? Email { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("UserId")] public virtual User User { get; set; } = null!;
        public virtual ICollection<PrescriptionDispense> PrescriptionDispenses { get; set; } = new List<PrescriptionDispense>();
    }
}
