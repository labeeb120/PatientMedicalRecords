using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PatientMedicalRecords.Models
{
    public class Surgery
    {
        [Key] public int Id { get; set; }
        [Required] public int PatientId { get; set; }
        [Required][StringLength(100)] public string SurgeryName { get; set; } = string.Empty;
        [StringLength(200)] public string? Description { get; set; }
        public DateTime SurgeryDate { get; set; }
        [StringLength(100)] public string? Hospital { get; set; }
        [StringLength(100)] public string? Surgeon { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("PatientId")] public virtual Patient Patient { get; set; } = null!;
    }
}
