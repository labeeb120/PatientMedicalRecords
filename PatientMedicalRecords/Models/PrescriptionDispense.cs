using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PatientMedicalRecords.Models
{
    public class PrescriptionDispense
    {
        [Key] public int Id { get; set; }
        [Required] public int PrescriptionId { get; set; }
        [Required] public int PharmacistId { get; set; }
        [StringLength(200)] public string? Notes { get; set; }
        public DateTime DispenseDate { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("PrescriptionId")] public virtual Prescription Prescription { get; set; } = null!;
        [ForeignKey("PharmacistId")] public virtual Pharmacist Pharmacist { get; set; } = null!;
    }
}
