using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PatientMedicalRecords.Models
{
    public enum UserRole { Patient = 1, Doctor = 2, Pharmacist = 3, Admin = 4 }
    public enum UserStatus { Pending = 1, Approved = 2, Rejected = 3, Suspended = 4 }
    public enum BloodType { A_Positive = 1, A_Negative = 2, B_Positive = 3, B_Negative = 4, AB_Positive = 5, AB_Negative = 6, O_Positive = 7, O_Negative = 8 }
    public enum Gender { Male = 1, Female = 2 }
    public enum PrescriptionStatus { Pending = 1, Dispensed = 2, PartiallyDispensed = 3, Cancelled = 4, InReview =5, NeedsConsultation =6}
    public enum AccessTokenStatus { Active = 1, Used = 2, Expired = 3, Revoked = 4 }
    //public enum PrescriptionStatus  {
    //    Pending,            // ÃœÌœ…°  ‰ Ÿ— «·„—«Ã⁄…
    //   ,           // ﬁÌœ «·„—«Ã⁄… „‰ ﬁ»· ’Ìœ·Ì
    //    ,  //   ÿ·» «” ‘«—… «·ÿ»Ì»
    //    Dispensed,          //  „ ’—›Â« »«·ﬂ«„·
    //    PartiallyDispensed, //  „ ’—›Â« Ã“∆Ì«
    //    Cancelled           // „·€«…
    //}



}