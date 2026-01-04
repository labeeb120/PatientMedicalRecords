using System.ComponentModel.DataAnnotations;
using PatientMedicalRecords.Models;

namespace PatientMedicalRecords.DTOs
{
    // Doctor Profile DTOs
    public class DoctorProfileRequest
    {
        [Required(ErrorMessage = "الاسم الكامل مطلوب")]
        [StringLength(100, ErrorMessage = "الاسم الكامل لا يجب أن يتجاوز 100 حرف")]
        public string FullName { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "التخصص لا يجب أن يتجاوز 50 حرف")]
        public string? Specialization { get; set; }

        [StringLength(20, ErrorMessage = "رقم الترخيص لا يجب أن يتجاوز 20 حرف")]
        public string? LicenseNumber { get; set; }

        [StringLength(100, ErrorMessage = "اسم المستشفى لا يجب أن يتجاوز 100 حرف")]
        public string? Hospital { get; set; }

        [StringLength(20, ErrorMessage = "رقم الهاتف لا يجب أن يتجاوز 20 رقم")]
        public string? PhoneNumber { get; set; }

        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
        [StringLength(100, ErrorMessage = "البريد الإلكتروني لا يجب أن يتجاوز 100 حرف")]
        public string? Email { get; set; }
    }

    public class DoctorProfileResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public DoctorInfo? Doctor { get; set; }
    }

    public class DoctorInfo
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Specialization { get; set; }
        public string? LicenseNumber { get; set; }
        public string? Hospital { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    // Patient Search DTOs
    public class PatientSearchRequest
    {

        /// <summary>
        /// الرقم الوطني للمريض (يمكن تركه فارغًا إذا استخدم PatientCode)
        /// </summary>
       ///
       /// [Required(ErrorMessage = "الرقم الوطني مطلوب")]
        //[StringLength(10, MinimumLength = 9, ErrorMessage = "الرقم الوطني يجب أن يكون بين 9-10 أرقام")]
        public string Identifier { get; set; } = string.Empty;

        ///// <summary>
        ///// كود المريض (PatientCode)، يمكن تركه فارغًا إذا استخدم NationalId
        ///// </summary>
        //public string PatientCode { get; set; }
    }

    public class PatientSearchResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public PatientMedicalInfo? Patient { get; set; }
    }

    public class PatientMedicalInfo
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public Gender Gender { get; set; }
        public BloodType? BloodType { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Height { get; set; }
        public string? EmergencyContact { get; set; }
        public string? EmergencyPhone { get; set; }
        public string? PatientCode { get; set; }
        public List<AllergyInfo> Allergies { get; set; } = new List<AllergyInfo>();
        public List<ChronicDiseaseInfo> ChronicDiseases { get; set; } = new List<ChronicDiseaseInfo>();
        public List<SurgeryInfo> Surgeries { get; set; } = new List<SurgeryInfo>();
        public List<MedicalRecordInfo> MedicalRecords { get; set; } = new List<MedicalRecordInfo>();
        public List<PrescriptionInfo>? Prescriptions { get; set; } =  new List<PrescriptionInfo>();
    }

    // Prescription DTOs
    public class PrescriptionRequest
    {
        [Required(ErrorMessage = "معرف المريض مطلوب")]
        public int PatientId { get; set; }

        [StringLength(200, ErrorMessage = "التشخيص لا يجب أن يتجاوز 200 حرف")]
        public string? Diagnosis { get; set; }

        [StringLength(1000, ErrorMessage = "الملاحظات لا يجب أن تتجاوز 1000 حرف")]
        public string? Notes { get; set; }

        public List<PrescriptionItemRequest> Items { get; set; } = new List<PrescriptionItemRequest>();
    }

    public class PrescriptionItemRequest
    {
        [Required(ErrorMessage = "اسم الدواء مطلوب")]
        [StringLength(100, ErrorMessage = "اسم الدواء لا يجب أن يتجاوز 100 حرف")]
        public string MedicationName { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "الجرعة لا يجب أن تتجاوز 50 حرف")]
        public string? Dosage { get; set; }

        [StringLength(50, ErrorMessage = "التكرار لا يجب أن يتجاوز 50 حرف")]
        public string? Frequency { get; set; }

        [StringLength(50, ErrorMessage = "المدة لا يجب أن تتجاوز 50 حرف")]
        public string? Duration { get; set; }

        [StringLength(200, ErrorMessage = "التعليمات لا يجب أن تتجاوز 200 حرف")]
        public string? Instructions { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "الكمية يجب أن تكون أكبر من 0")]
        public int Quantity { get; set; }
    }

    public class PrescriptionResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public PrescriptionInfo? Prescription { get; set; }
    }

    public class PrescriptionInfo
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int? DoctorId { get; set; }
        public string? Diagnosis { get; set; }
        public string? Notes { get; set; }
        public PrescriptionStatus Status { get; set; }
        public DateTime PrescriptionDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? DoctorName { get; set; }
        public string? PatientName { get; set; }
        public List<PrescriptionItemInfo> Items { get; set; } = new List<PrescriptionItemInfo>();
    }

    public class PrescriptionItemInfo
    {
        public int Id { get; set; }
        public int PrescriptionId { get; set; }
        public string MedicationName { get; set; } = string.Empty;
        public string? Dosage { get; set; }
        public string? Frequency { get; set; }
        public string? Duration { get; set; }
        public string? Instructions { get; set; }
        public int Quantity { get; set; }
        public bool IsDispensed { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Access Request DTOs
    public class AccessRequestResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int RequestId { get; set; }
        public bool RequiresApproval { get; set; }
    }

    public class AccessApprovalRequest
    {
        [Required(ErrorMessage = "معرف الطلب مطلوب")]
        public int RequestId { get; set; }

        [Required(ErrorMessage = "القرار مطلوب")]
        public bool Approved { get; set; }

        public string? Notes { get; set; }
    }

    public class AccessApprovalResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
