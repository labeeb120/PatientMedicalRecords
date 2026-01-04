using PatientMedicalRecords.Models;
using System.ComponentModel.DataAnnotations;

namespace PatientMedicalRecords.Models
{
    
    public class Drug
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string ScientificName { get; set; } = string.Empty; // الاسم العلمي

        [StringLength(100)]
        public string? BrandName { get; set; } // الاسم التجاري

        [StringLength(100)]
        public string? ChemicalName { get; set; } // الاسم الكيميائي

        [StringLength(200)]
        public string? Manufacturer { get; set; }

        [StringLength(200)]
        public string? NormalizedName { get; set; } // للاستخدام في البحث والمقارنة

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // العلاقة بين الدواء والمركبات النشطة (Ingredients)
        public virtual ICollection<DrugIngredient> DrugIngredients { get; set; } = new List<DrugIngredient>();

        // العلاقة بين الدواء وعناصر الوصفات
        public virtual ICollection<PrescriptionItem> PrescriptionItems { get; set; } = new List<PrescriptionItem>();
    }
}



//public class Drug
//{
//    [Key]
//    public int Id { get; set; }

//    public string BrandName { get; set; } = null!;
//    public string GenericName { get; set; } = null!;
//    public string Manufacturer { get; set; } = null!;
//    public string DrugClass { get; set; } = null!;
//    public string ScientificName { get; set; }
//    public string ChemicalName { get; set; } = null!;

//    // علاقة عدة مواد فعّالة للدواء الواحد
//    public ICollection<DrugIngredient> DrugIngredients { get; set; } = new List<DrugIngredient>();
//}