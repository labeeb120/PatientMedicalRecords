using System.ComponentModel.DataAnnotations.Schema;

namespace PatientMedicalRecords.Models
{
    // هذا هو الموديل الذي يمثل الجدول في قاعدة البيانات
    public class UserRoleAssignment
    {
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public virtual User User { get; set; }

        // *** النقطة الأهم ***
        // الحقل هنا يستخدم الـ enum القديم الذي لم نغيره
        public UserRole Role { get; set; }
    }
}
