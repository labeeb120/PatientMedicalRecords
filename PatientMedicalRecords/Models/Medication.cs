namespace PatientMedicalRecords.Models
{
    public class Medication
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // اسم العرض (قد يكون تجاري أو عام)
        public string NormalizedName { get; set; } = string.Empty; // normalized for lookup (lower/trim)
        public ICollection<MedicationIngredient> MedicationIngredients { get; set; } = new List<MedicationIngredient>();
    }
}

