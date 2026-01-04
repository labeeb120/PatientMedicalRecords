using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PatientMedicalRecords.Models
{
    public class DrugInteraction
    {
      

       
        [Key] public int Id { get; set; }
        //// لضمان التفرد وتجنب تكرار التفاعل (نستخدم دائماً الأصغر في A والأكبر في B)

        [Required] public int IngredientAId { get; set; }
        [Required] public int IngredientBId { get; set; } // المفاتيح الخارجية يجب أن تكون موجودة

        [Required]
        [StringLength(50)]
        public string Severity { get; set; } = string.Empty;


        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Recommendation { get; set; } // الإجراء الموصى به

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //[ForeignKey("IngredientAId")]
        //public virtual Ingredient IngredientA { get; set; } = null!;

        //[ForeignKey("IngredientBId")]
        //public virtual Ingredient IngredientB { get; set; } = null!;
        [ForeignKey("IngredientAId")] // ربط الخاصية Navigation Property بالمفتاح الخارجي
        public virtual Ingredient IngredientA { get; set; } = null!;

        [ForeignKey("IngredientBId")] // ربط الخاصية Navigation Property بالمفتاح الخارجي
        public virtual Ingredient IngredientB { get; set; } = null!;
    }
}
