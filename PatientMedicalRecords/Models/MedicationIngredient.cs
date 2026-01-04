namespace PatientMedicalRecords.Models
{
    // Models/MedicationIngredient.cs (many-to-many)
    public class MedicationIngredient
    {
        public int MedicationId { get; set; }
        public Medication Medication { get; set; } = null!;
        public int IngredientId { get; set; }
        public Ingredient Ingredient { get; set; } = null!;
    }
}


