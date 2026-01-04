using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PatientMedicalRecords.Models
{
    public class Allergy
    {
        [Key] public int Id { get; set; }
        [Required] public int PatientId { get; set; }
        [Required][StringLength(100)] public string AllergenName { get; set; } = string.Empty;
        [StringLength(200)] public string? Reaction { get; set; }
        [StringLength(50)] public string? Severity { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("PatientId")] public virtual Patient Patient { get; set; } = null!;
    }
}
