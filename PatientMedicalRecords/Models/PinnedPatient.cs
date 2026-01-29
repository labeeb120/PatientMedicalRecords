using System.ComponentModel.DataAnnotations.Schema;
//25-01-2026
namespace PatientMedicalRecords.Models
{
    public class PinnedPatient
    {
        public int Id { get; set; }

        [ForeignKey("Doctor")]
        public int DoctorId { get; set; }
        public virtual Doctor Doctor { get; set; }

        [ForeignKey("Patient")]
        public int PatientId { get; set; }
        public virtual Patient Patient { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
