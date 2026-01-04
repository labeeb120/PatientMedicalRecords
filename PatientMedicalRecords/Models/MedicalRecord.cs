using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PatientMedicalRecords.Models
{
    public class MedicalRecord
    {
        [Key] public int Id { get; set; }
        [Required] public int PatientId { get; set; }
        public int? DoctorId { get; set; }
        [StringLength(200)] public string? Diagnosis { get; set; }
        [StringLength(1000)] public string? Notes { get; set; }
        [StringLength(100)] public string? Symptoms { get; set; }
        [StringLength(100)] public string? Treatment { get; set; }
        public DateTime RecordDate { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("PatientId")] public virtual Patient Patient { get; set; } = null!;
        [ForeignKey("DoctorId")] public virtual Doctor Doctor { get; set; } = null!;
    }
}
