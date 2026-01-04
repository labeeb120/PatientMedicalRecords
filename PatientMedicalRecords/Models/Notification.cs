using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PatientMedicalRecords.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")] public virtual User User { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Body { get; set; }

        public string? DataJson { get; set; } // أي بيانات إضافية (مثلاً: {"action":"open_profile","patientCode":"PM-..."} )

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
