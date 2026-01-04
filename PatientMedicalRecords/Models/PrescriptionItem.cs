using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PatientMedicalRecords.Models
{
    public class PrescriptionItem
    {
        [Key] public int Id { get; set; }
        [Required] public int PrescriptionId { get; set; }
        [Required][StringLength(100)]
        public string MedicationName { get; set; } = string.Empty;
        [StringLength(50)]
        public string? Dosage { get; set; } = string.Empty;
        [StringLength(50)] 
        public string? Frequency { get; set; } = string.Empty;
        [StringLength(50)] 
        public string? Duration { get; set; } = string.Empty;
        [StringLength(200)] 
        public string? Instructions { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public bool IsDispensed { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required] public int DrugId { get; set; } // جعلها مطلوبة
        [ForeignKey("DrugId")]
        public virtual Drug Drug { get; set; } = null!; // تغيير النوع إلى Drug

        [ForeignKey("PrescriptionId")]
        public virtual Prescription Prescription { get; set; } = null!;
    }

    

}

//**************************************
//public class PrescriptionItem
//{
//    [Key] public int Id { get; set; }
//    [Required] public int PrescriptionId { get; set; }

//    // أبقينا MedicationName لأنه قد يتم إدخاله يدوياً في الأنظمة غير المكتملة
//    [Required]
//    [StringLength(100)]
//    public string MedicationName { get; set; } = string.Empty;

//    // ... (باقي الخصائص مثل Dosage, Frequency, Duration, Instructions, Quantity, IsDispensed, CreatedAt)

//    // الربط بالدواء المخزن في قاعدة البيانات
//    [Required] public int DrugId { get; set; } // جعلها مطلوبة
//    [ForeignKey("DrugId")]
//    public virtual Drug Drug { get; set; } = null!; // تغيير النوع إلى Drug

//    [ForeignKey("PrescriptionId")]
//    public virtual Prescription Prescription { get; set; } = null!;
//}