using PatientMedicalRecords.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PatientMedicalRecords.Models
{
    // Models/IngredientInteraction.cs
    public class IngredientInteraction
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey(nameof(Id))]
        public virtual Ingredient? Ingredient { get; set; }

        // store with invariant order: smaller id first to prevent duplicates
        public int IngredientAId { get; set; }
        [NotMapped] public Ingredient? IngredientA { get; set; } = null!;


        public int IngredientBId { get; set; }
        [NotMapped] public Ingredient? IngredientB { get; set; } = null!;

        public int PrimaryIngredientId { get; set; }
        [NotMapped] public Ingredient? PrimaryIngredient { get; set; }

        public int InteractsWithIngredientId { get; set; }
        [NotMapped] public Ingredient? InteractsWithIngredient { get; set; }

        public string Severity { get; set; } = string.Empty; // e.g., "High", "Moderate", "Low"
        public string Description { get; set; } = string.Empty;
        public string Recommendation { get; set; } = string.Empty;
    }          
}


