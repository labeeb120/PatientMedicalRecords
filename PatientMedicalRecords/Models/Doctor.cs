using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PatientMedicalRecords.Models
{
    public class Doctor
    {
        [Key] 
        [Required] public int UserId { get; set; }
        [Required][StringLength(100)] public string FullName { get; set; } = string.Empty;
        [StringLength(50)] public string? Specialization { get; set; }
        [StringLength(20)] public string? LicenseNumber { get; set; }
        [StringLength(100)] public string? Hospital { get; set; }
        [StringLength(20)] public string? PhoneNumber { get; set; }
        [StringLength(100)] public string? Email { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("UserId")] public virtual User User { get; set; } = null!;
        public virtual ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
        public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    }
}
