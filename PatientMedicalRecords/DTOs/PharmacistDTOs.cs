using System.ComponentModel.DataAnnotations;
using PatientMedicalRecords.DTOs;
using PatientMedicalRecords.Models;

namespace PatientMedicalRecords.DTOs
{
    // Pharmacist Profile DTOs
    public class PharmacistProfileRequest
    {
        [Required(ErrorMessage = "الاسم الكامل مطلوب")]
        [StringLength(100, ErrorMessage = "الاسم الكامل لا يجب أن يتجاوز 100 حرف")]
        public string FullName { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "رقم الترخيص لا يجب أن يتجاوز 20 حرف")]
        public string? LicenseNumber { get; set; }

        [StringLength(100, ErrorMessage = "اسم الصيدلية لا يجب أن يتجاوز 100 حرف")]
        public string? PharmacyName { get; set; }

        [StringLength(20, ErrorMessage = "رقم الهاتف لا يجب أن يتجاوز 20 رقم")]
        public string? PhoneNumber { get; set; }

        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
        [StringLength(100, ErrorMessage = "البريد الإلكتروني لا يجب أن يتجاوز 100 حرف")]
        public string? Email { get; set; }
    }

    public class PharmacistProfileResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public PharmacistInfo? Pharmacist { get; set; }
    }

    public class PharmacistInfo
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? LicenseNumber { get; set; }
        public string? PharmacyName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    // Prescription Dispense DTOs
    public class PrescriptionDispenseRequest
    {
        [Required(ErrorMessage = "معرف الوصفة مطلوب")]
        public int PrescriptionId { get; set; }

        [StringLength(200, ErrorMessage = "الملاحظات لا يجب أن تتجاوز 200 حرف")]
        public string? Notes { get; set; }

        public List<DispenseItemRequest> Items { get; set; } = new List<DispenseItemRequest>();
    }

    public class DispenseItemRequest
    {
        [Required(ErrorMessage = "معرف عنصر الوصفة مطلوب")]
        public int PrescriptionItemId { get; set; }

        [Required(ErrorMessage = "قرار الصرف مطلوب")]
        public bool Dispensed { get; set; }

        [StringLength(200, ErrorMessage = "ملاحظات الصرف لا يجب أن تتجاوز 200 حرف")]
        public string? Notes { get; set; }
    }

    public class PrescriptionDispenseResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public PrescriptionDispenseInfo? Dispense { get; set; }
        public List<DrugInteractionWarning>? Warnings { get; set; }
    }

    public class PrescriptionDispenseInfo
    {
        public int Id { get; set; }
        public int PrescriptionId { get; set; }
        public int PharmacistId { get; set; }
        public string? Notes { get; set; }
        public DateTime DispenseDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? PharmacistName { get; set; }
        public List<DispensedItemInfo> Items { get; set; } = new List<DispensedItemInfo>();
    }

    public class DispensedItemInfo
    {
        public int Id { get; set; }
        public int PrescriptionItemId { get; set; }
        public string MedicationName { get; set; } = string.Empty;
        public string? Dosage { get; set; }
        public string? Frequency { get; set; }
        public string? Duration { get; set; }
        public string? Instructions { get; set; }
        public int Quantity { get; set; }
        public bool IsDispensed { get; set; }
        public string? DispenseNotes { get; set; }
    }

    // Drug Interaction DTOs
    /// <summary>
    /// تفاعلات الادوية 
    /// </summary>


    //************************************************
    // DTO للتحذيرات من التفاعلات
    // DTOs/DrugInteractionDtos.cs
    public class DrugInteractionWarning
    {
        public string Medication1 { get; set; } = string.Empty; // الاسم الذي أدخله المستخدم
        public string Medication2 { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Recommendation { get; set; } = string.Empty;
    }

    public class DrugComparisonStep
    {
        public string IngredientA { get; set; }
        public string IngredientB { get; set; }
        public bool InteractionFound { get; set; }
    }

    public class DrugInteractionCheckRequest
    {
        [Required(ErrorMessage = "معرف المريض مطلوب")]
        public int PatientId { get; set; }
        public int PrescriptionId { get; set; }
        /// <summary>
        /// قائمة أسماء الأدوية كما أدخلها الطبيب (يمكن أن تكون اسم تجاري أو اسم عام)
        /// </summary>
        [Required(ErrorMessage = "قائمة الأدوية مطلوبة")]
        public List<string> Medications { get; set; } = new List<string>();
    }
    // رد الفحص
    public class DrugInteractionCheckResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<DrugInteractionWarning> Warnings { get; set; } = new List<DrugInteractionWarning>();
        public bool HasInteractions { get; set; }
        public List<DrugComparisonStep> ComparisonSteps { get; set; }
        
    }


}





    // اقتراحات الأدوية للطبيب
    public class DrugSuggestionDto
    {
        public int DrugId { get; set; }
        public string ScientificName { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
        public string ChemicalName { get; set; } = string.Empty;
    }

    // DTO لإضافة وصفة جديدة
    public class PrescriptionCreateRequest
    {
        [Required]
        public int PatientId { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [Required]
        public string Diagnosis { get; set; } = string.Empty;

        [Required]
        public List<PrescriptionItemDto> Items { get; set; } = new List<PrescriptionItemDto>();
    }

    // كل صنف دوائي في الوصفة
    public class PrescriptionItemDto
    {
        [Required]
        public int DrugId { get; set; }
        [Required] public int Quantity { get; set; }

    [Required]
        public string Dosage { get; set; } = string.Empty;

        [Required]
        public string Frequency { get; set; } = string.Empty;

        [Required]
        public string Duration { get; set; } = string.Empty;

        public string Instructions { get; set; } = string.Empty;
    }

 
    public class PrescriptionSearchRequest
    {
        /// <summary>
        /// الرقم الوطني للمريض (يمكن تركه فارغًا إذا استخدم PatientCode)
        /// </summary>
        ///        
        public string Identifier { get; set; } = string.Empty;
    }

    public class PrescriptionSearchResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<PrescriptionInfo> Prescriptions { get; set; } = new List<PrescriptionInfo>();
    }

    // Inventory DTOs
    public class InventoryItem
    {
        public string MedicationName { get; set; } = string.Empty;
        public int AvailableQuantity { get; set; }
        public int RequiredQuantity { get; set; }
        public bool IsAvailable { get; set; }
        public decimal? UnitPrice { get; set; }
    }

    public class InventoryCheckRequest
    {
        [Required(ErrorMessage = "معرف الوصفة مطلوب")]
        public int PrescriptionId { get; set; }
    }

    public class InventoryCheckResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<InventoryItem> Items { get; set; } = new List<InventoryItem>();
        public bool AllItemsAvailable { get; set; }
        public decimal? TotalCost { get; set; }
    }

    // Prescription Status Update DTOs
    public class PrescriptionStatusUpdateRequest
    {
        [Required(ErrorMessage = "معرف الوصفة مطلوب")]
        public int PrescriptionId { get; set; }

        [Required(ErrorMessage = "الحالة الجديدة مطلوبة")]
        public PrescriptionStatus Status { get; set; }

        [StringLength(200, ErrorMessage = "الملاحظات لا يجب أن تتجاوز 200 حرف")]
        public string? Notes { get; set; }
    }

    public class PrescriptionStatusUpdateResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public PrescriptionStatus NewStatus { get; set; }
    }






//*************************************************
// DTOs/DrugInteractionDtos.cs
//public class DrugInteractionWarning
//{
//    public string Medication1 { get; set; } = string.Empty; // الاسم الذي أدخله المستخدم
//    public string Medication2 { get; set; } = string.Empty;
//    public string Severity { get; set; } = string.Empty;
//    public string Description { get; set; } = string.Empty;
//    public string Recommendation { get; set; } = string.Empty;
//}

//public class DrugInteractionCheckRequest
//{
//    [Required(ErrorMessage = "معرف المريض مطلوب")]
//    public int PatientId { get; set; }

//    /// <summary>
//    /// قائمة أسماء الأدوية كما أدخلها الطبيب (يمكن أن تكون اسم تجاري أو اسم عام)
//    /// </summary>
//    [Required(ErrorMessage = "قائمة الأدوية مطلوبة")]
//    public List<string> Medications { get; set; } = new List<string>();
//}

//public class DrugInteractionCheckResponse
//{
//    public bool Success { get; set; }
//    public string Message { get; set; } = string.Empty;
//    public List<DrugInteractionWarning> Warnings { get; set; } = new List<DrugInteractionWarning>();
//    public bool HasInteractions { get; set; }
//}
















//public class DrugInteractionWarning
//{
//    public string Medication1 { get; set; } = string.Empty;
//    public string Medication2 { get; set; } = string.Empty;
//    public string Severity { get; set; } = string.Empty;
//    public string Description { get; set; } = string.Empty;
//    public string Recommendation { get; set; } = string.Empty;
//}

//public class DrugInteractionCheckRequest
//{
//    [Required(ErrorMessage = "معرف المريض مطلوب")]
//    public int PatientId { get; set; }

//    [Required(ErrorMessage = "قائمة الأدوية مطلوبة")]
//    public List<string> Medications { get; set; } = new List<string>();
//}

//public class DrugInteractionCheckResponse
//{
//    public bool Success { get; set; }
//    public string Message { get; set; } = string.Empty;
//    public List<DrugInteractionWarning> Warnings { get; set; } = new List<DrugInteractionWarning>();
//    public bool HasInteractions { get; set; }
//}

// Prescription Search DTOs