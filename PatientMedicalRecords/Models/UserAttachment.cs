using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace PatientMedicalRecords.Models
{
    public class UserAttachment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [Required]
        [StringLength(100)]
        public string AttachmentType { get; set; } // e.g., "LicenseCertificate", "NationalID"

        [Required]
        [StringLength(255)]
        public string FileName { get; set; } // Original file name

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; } // Path on the server or cloud storage URL

        [Required]
        [StringLength(50)]
        public string ContentType { get; set; } // MIME type, e.g., "application/pdf"

        public long FileSize { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
