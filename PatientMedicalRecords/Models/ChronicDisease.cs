using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PatientMedicalRecords.Models
{
    public class ChronicDisease
    {
        [Key] public int Id { get; set; }
        [Required] public int PatientId { get; set; }
        [Required][StringLength(100)] public string DiseaseName { get; set; } = string.Empty;
        [StringLength(200)] public string? Description { get; set; }
        public DateTime? DiagnosisDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("PatientId")] public virtual Patient Patient { get; set; } = null!;
    }
}
