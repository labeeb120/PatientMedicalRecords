using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PatientMedicalRecords.Models
{
    public class Prescription
    {
        [Key] public int Id { get; set; }
        [Required] public int PatientId { get; set; }
        [ForeignKey("PatientId")] public virtual Patient Patient { get; set; } = null!;

        public int? DoctorId { get; set; }
        [ForeignKey("DoctorId")] public virtual Doctor Doctor { get; set; } = null!;

        [StringLength(200)] 
        public string Diagnosis { get; set; } = string.Empty;
        [StringLength(1000)] public string? Notes { get; set; }

        public DateTime PrescriptionDate { get; set; } = DateTime.UtcNow;
        public PrescriptionStatus Status { get; set; } = PrescriptionStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<PrescriptionItem> PrescriptionItems { get; set; } = new List<PrescriptionItem>();
        public virtual ICollection<PrescriptionDispense> PrescriptionDispenses { get; set; } = new List<PrescriptionDispense>();
    }
       
    
}
