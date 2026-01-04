using PatientMedicalRecords.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PatientMedicalRecords.Models
{   
    public class Ingredient
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty; // اسم المركب النشط

        [Required]
        [StringLength(100)]
        public string NormalizedName { get; set; } = string.Empty; // للاستخدام في فحص التفاعلات

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // العلاقة بالنماذج الأخرى
        public virtual ICollection<DrugIngredient> DrugIngredients { get; set; } = new List<DrugIngredient>();
        public virtual ICollection<DrugInteraction> DrugInteractionAsA { get; set; } = new List<DrugInteraction>();
        public virtual ICollection<DrugInteraction> DrugInteractionAsB { get; set; } = new List<DrugInteraction>();
    }

}


//public class Ingredient
//{
//    public int Id { get; set; }
//    public string Name { get; set; } = string.Empty; // e.g., "warfarin"
//    public string NormalizedName { get; set; } = string.Empty;
//    public string ChemicalFormula { get; set; } = null!;
//    public string Description { get; set; } = null!;
//    public ICollection<MedicationIngredient> MedicationIngredients { get; set; } = new List<MedicationIngredient>();
//    public ICollection<DrugIngredient> DrugIngredients { get; set; } = new List<DrugIngredient>();
//    [NotMapped]
//    public ICollection<IngredientInteraction> InteractionsAsPrimary { get; set; } = new List<IngredientInteraction>();
//    [NotMapped]
//    public ICollection<IngredientInteraction> InteractionsAsInteractsWith { get; set; } = new List<IngredientInteraction>();
//}