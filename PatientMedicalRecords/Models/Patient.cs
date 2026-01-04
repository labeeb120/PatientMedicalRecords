using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PatientMedicalRecords.Models
{
    public class Patient
    {
        [Key]
        [Required] public int UserId { get; set; }
        [Required][StringLength(100)] public string FullName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        [Required] public Gender Gender { get; set; }
        [StringLength(20)] public string? PhoneNumber { get; set; }
        [StringLength(100)] public string? Email { get; set; }
        [StringLength(200)] public string? Address { get; set; }
        public BloodType? BloodType { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Height { get; set; }
        [StringLength(20)] public string? EmergencyContact { get; set; }
        [StringLength(100)] public string? EmergencyPhone { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        [StringLength(20)]
        public string? PatientCode { get; set; } // مثال: PM-3F9A72A1

        [ForeignKey("UserId")] public virtual User User { get; set; } = null!;
        public virtual ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
        public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
        public virtual ICollection<Allergy> Allergies { get; set; } = new List<Allergy>();
        public virtual ICollection<ChronicDisease> ChronicDiseases { get; set; } = new List<ChronicDisease>();
        public virtual ICollection<Surgery> Surgeries { get; set; } = new List<Surgery>();
    }
}
