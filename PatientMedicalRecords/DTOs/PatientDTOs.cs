using System.ComponentModel.DataAnnotations;
using PatientMedicalRecords.Models;


namespace PatientMedicalRecords.DTOs
{
    // Initialize Patient Profile (first-time)
    public class InitializePatientProfileRequest
{
    [Required(ErrorMessage = "الاسم الكامل مطلوب")]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "تاريخ الميلاد مطلوب")]
    public DateTime DateOfBirth { get; set; }

    [Required(ErrorMessage = "الجنس مطلوب")]
    public Gender Gender { get; set; }

    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(200)]
    public string? Address { get; set; }

    public BloodType? BloodType { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Height { get; set; }

    [StringLength(20)]
    public string? EmergencyContact { get; set; }

    [StringLength(100)]
    public string? EmergencyPhone { get; set; }

    public List<AllergyInput> Allergies { get; set; } = new();
    public List<ChronicDiseaseInput> ChronicDiseases { get; set; } = new();
    public List<SurgeryInput> Surgeries { get; set; } = new();
    public List<CurrentMedicationInput> CurrentMedications { get; set; } = new();

    [StringLength(1000)]
    public string? Notes { get; set; }
}

public class AllergyInput
{
    [Required]
    [StringLength(100)]
    public string AllergenName { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Reaction { get; set; }

    [StringLength(50)]
    public string? Severity { get; set; }
}

public class ChronicDiseaseInput
{
    [Required]
    [StringLength(100)]
    public string DiseaseName { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Description { get; set; }

    public DateTime? DiagnosisDate { get; set; }
}

public class SurgeryInput
{
    [Required]
    [StringLength(100)]
    public string SurgeryName { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Description { get; set; }

    [Required]
    public DateTime SurgeryDate { get; set; }

    [StringLength(100)]
    public string? Hospital { get; set; }

    [StringLength(100)]
    public string? Surgeon { get; set; }
}

public class CurrentMedicationInput
{
    [Required]
    [StringLength(100)]
    public string MedicationName { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Dosage { get; set; }

    [StringLength(50)]
    public string? Frequency { get; set; }

    [StringLength(50)]
    public string? Duration { get; set; }

    [StringLength(200)]
    public string? Instructions { get; set; }
}

public class InitializePatientProfileResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int PatientId { get; set; }
}

// Patient Profile DTOs
public class PatientProfileRequest
{
    [Required(ErrorMessage = "الاسم الكامل مطلوب")]
    [StringLength(100, ErrorMessage = "الاسم الكامل لا يجب أن يتجاوز 100 حرف")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "تاريخ الميلاد مطلوب")]
    public DateTime DateOfBirth { get; set; }

    [Required(ErrorMessage = "الجنس مطلوب")]
    public Gender Gender { get; set; }

    [StringLength(20, ErrorMessage = "رقم الهاتف لا يجب أن يتجاوز 20 رقم")]
    public string? PhoneNumber { get; set; }

    [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
    [StringLength(100, ErrorMessage = "البريد الإلكتروني لا يجب أن يتجاوز 100 حرف")]
    public string? Email { get; set; }

    [StringLength(200, ErrorMessage = "العنوان لا يجب أن يتجاوز 200 حرف")]
    public string? Address { get; set; }

    public BloodType? BloodType { get; set; }

    [Range(0, 500, ErrorMessage = "الوزن يجب أن يكون بين 0 و 500 كيلو")]
    public decimal? Weight { get; set; }

    [Range(0, 300, ErrorMessage = "الطول يجب أن يكون بين 0 و 300 سم")]
    public decimal? Height { get; set; }

    [StringLength(20, ErrorMessage = "رقم الطوارئ لا يجب أن يتجاوز 20 رقم")]
    public string? EmergencyContact { get; set; }

    [StringLength(100, ErrorMessage = "اسم جهة الاتصال في الطوارئ لا يجب أن يتجاوز 100 حرف")]
    public string? EmergencyPhone { get; set; }
}

public class PatientProfileResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public PatientInfo? Patient { get; set; }
}

public class PatientInfo
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public BloodType? BloodType { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Height { get; set; }
    public string? EmergencyContact { get; set; }
    public string? EmergencyPhone { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

// Medical Record DTOs
public class MedicalRecordRequest
{
    [Required(ErrorMessage = "معرف المريض مطلوب")]
    public int PatientId { get; set; }

    [StringLength(200, ErrorMessage = "التشخيص لا يجب أن يتجاوز 200 حرف")]
    public string? Diagnosis { get; set; }

    [StringLength(1000, ErrorMessage = "الملاحظات لا يجب أن تتجاوز 1000 حرف")]
    public string? Notes { get; set; }

    [StringLength(100, ErrorMessage = "الأعراض لا يجب أن تتجاوز 100 حرف")]
    public string? Symptoms { get; set; }

    [StringLength(100, ErrorMessage = "العلاج لا يجب أن يتجاوز 100 حرف")]
    public string? Treatment { get; set; }

    public DateTime RecordDate { get; set; } = DateTime.UtcNow;
}

public class MedicalRecordResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public MedicalRecordInfo? MedicalRecord { get; set; }
}

public class MedicalRecordInfo
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int? DoctorId { get; set; }
    public string? Diagnosis { get; set; }
    public string? Notes { get; set; }
    public string? Symptoms { get; set; }
    public string? Treatment { get; set; }
    public DateTime RecordDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? DoctorName { get; set; }
    public string? PatientName { get; set; }
}

// Allergy DTOs
public class AllergyRequest
{
    [Required(ErrorMessage = "معرف المريض مطلوب")]
    public int PatientId { get; set; }

    [Required(ErrorMessage = "اسم المادة المسببة للحساسية مطلوب")]
    [StringLength(100, ErrorMessage = "اسم المادة المسببة للحساسية لا يجب أن يتجاوز 100 حرف")]
    public string AllergenName { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "رد الفعل لا يجب أن يتجاوز 200 حرف")]
    public string? Reaction { get; set; }

    [StringLength(50, ErrorMessage = "الخطورة لا يجب أن تتجاوز 50 حرف")]
    public string? Severity { get; set; }
}

public class AllergyResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public AllergyInfo? Allergy { get; set; }
}

public class AllergyInfo
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string AllergenName { get; set; } = string.Empty;
    public string? Reaction { get; set; }
    public string? Severity { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Chronic Disease DTOs
public class ChronicDiseaseRequest
{
    [Required(ErrorMessage = "معرف المريض مطلوب")]
    public int PatientId { get; set; }

    [Required(ErrorMessage = "اسم المرض مطلوب")]
    [StringLength(100, ErrorMessage = "اسم المرض لا يجب أن يتجاوز 100 حرف")]
    public string DiseaseName { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "الوصف لا يجب أن يتجاوز 200 حرف")]
    public string? Description { get; set; }

    public DateTime? DiagnosisDate { get; set; }
}

public class ChronicDiseaseResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public ChronicDiseaseInfo? ChronicDisease { get; set; }
}

public class ChronicDiseaseInfo
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string DiseaseName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DiagnosisDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Surgery DTOs
public class SurgeryRequest
{
    [Required(ErrorMessage = "معرف المريض مطلوب")]
    public int PatientId { get; set; }

    [Required(ErrorMessage = "اسم العملية مطلوب")]
    [StringLength(100, ErrorMessage = "اسم العملية لا يجب أن يتجاوز 100 حرف")]
    public string SurgeryName { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "الوصف لا يجب أن يتجاوز 200 حرف")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "تاريخ العملية مطلوب")]
    public DateTime SurgeryDate { get; set; }

    [StringLength(100, ErrorMessage = "اسم المستشفى لا يجب أن يتجاوز 100 حرف")]
    public string? Hospital { get; set; }

    [StringLength(100, ErrorMessage = "اسم الجراح لا يجب أن يتجاوز 100 حرف")]
    public string? Surgeon { get; set; }
}

public class SurgeryResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public SurgeryInfo? Surgery { get; set; }
}

public class SurgeryInfo
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string SurgeryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime SurgeryDate { get; set; }
    public string? Hospital { get; set; }
    public string? Surgeon { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Emergency Screen DTOs
public class EmergencyScreenResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public EmergencyInfo? EmergencyInfo { get; set; }
}

public class EmergencyInfo
{
    public string FullName { get; set; } = string.Empty;
    public BloodType? BloodType { get; set; }
    public List<string> Allergies { get; set; } = new List<string>();
    public List<string> ChronicDiseases { get; set; } = new List<string>();
    public List<string> CurrentMedications { get; set; } = new List<string>();
    public string? EmergencyContact { get; set; }
    public string? EmergencyPhone { get; set; }
    public string QRCodeUrl { get; set; } = string.Empty;
}





}




























