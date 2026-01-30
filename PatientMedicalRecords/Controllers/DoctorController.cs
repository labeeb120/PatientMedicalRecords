using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatientMedicalRecords.Data;
using PatientMedicalRecords.DTOs;
using PatientMedicalRecords.Models;
using PatientMedicalRecords.Services;
using System.Security.Claims;

namespace PatientMedicalRecords.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DoctorController : ControllerBase
    {
        private readonly MedicalRecordsDbContext _context;
        private readonly IQRCodeService _qrCodeService;
        private readonly IDrugInteractionService _drugInteractionService;        
        private readonly ILogger<DoctorController> _logger;
        private readonly IPatientDataService _patientDataService;
        private readonly IJwtService _jwtService;

        public DoctorController(
            MedicalRecordsDbContext context,
            IQRCodeService qrCodeService,
            IDrugInteractionService drugInteractionService,            
            ILogger<DoctorController> logger,
            IPatientDataService patientDataService,
            IJwtService jwtService
            )
        {
            _context = context;
            _qrCodeService = qrCodeService;
            _drugInteractionService = drugInteractionService;            
            _logger = logger;
            _patientDataService = patientDataService;
            _jwtService = jwtService;
        }

        /// <summary>
        /// الحصول على ملف الطبيب الشخصي
        /// </summary>
        [HttpGet("profile")]
        public async Task<ActionResult<DoctorProfileResponse>> GetProfile()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var doctor = await _context.Doctors
                    .Include(d => d.User)
                    .FirstOrDefaultAsync(d => d.UserId == userId);

                if (doctor == null)
                {
                    return NotFound(new DoctorProfileResponse
                    {
                        Success = false,
                        Message = "لم يتم العثور على ملف الطبيب"
                    });
                }

                var doctorInfo = new DoctorInfo
                {
                    Id = doctor.UserId,
                    UserId = doctor.UserId,
                    FullName = doctor.FullName,
                    Specialization = doctor.Specialization,
                    LicenseNumber = doctor.LicenseNumber,
                    Hospital = doctor.Hospital,
                    PhoneNumber = doctor.PhoneNumber,
                    Email = doctor.Email,
                    CreatedAt = doctor.CreatedAt,
                    UpdatedAt = doctor.UpdatedAt
                };

                return Ok(new DoctorProfileResponse
                {
                    Success = true,
                    Message = "تم جلب ملف الطبيب بنجاح",
                    Doctor = doctorInfo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting doctor profile");
                return StatusCode(500, new DoctorProfileResponse
                {
                    Success = false,
                    Message = "حدث خطأ في الخادم"
                });
            }
        }

        /// <summary>
        /// تحديث ملف الطبيب الشخصي
        /// </summary>
        [HttpPut("profile")]
        public async Task<ActionResult<DoctorProfileResponse>> UpdateProfile([FromBody] DoctorProfileRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new DoctorProfileResponse
                    {
                        Success = false,
                        Message = "البيانات المدخلة غير صحيحة"
                    });
                }

                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var doctor = await _context.Doctors
                    .FirstOrDefaultAsync(d => d.UserId == userId);

                if (doctor == null)
                {
                    return NotFound(new DoctorProfileResponse
                    {
                        Success = false,
                        Message = "لم يتم العثور على ملف الطبيب"
                    });
                }

                //Update doctor information
                doctor.FullName = request.FullName;
                doctor.Specialization = request.Specialization;
                doctor.LicenseNumber = request.LicenseNumber;
                doctor.Hospital = request.Hospital;
                doctor.PhoneNumber = request.PhoneNumber;
                doctor.Email = request.Email;
                doctor.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                //Log the update
               await LogUserAction(userId.Value, "UPDATE_PROFILE", "تم تحديث ملف الطبيب الشخصي");

                var doctorInfo = new DoctorInfo
                {
                    Id = doctor.UserId,
                    UserId = doctor.UserId,
                    FullName = doctor.FullName,
                    Specialization = doctor.Specialization,
                    LicenseNumber = doctor.LicenseNumber,
                    Hospital = doctor.Hospital,
                    PhoneNumber = doctor.PhoneNumber,
                    Email = doctor.Email,
                    CreatedAt = doctor.CreatedAt,
                    UpdatedAt = doctor.UpdatedAt
                };

                return Ok(new DoctorProfileResponse
                {
                    Success = true,
                    Message = "تم تحديث ملف الطبيب بنجاح",
                    Doctor = doctorInfo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating doctor profile");
                return StatusCode(500, new DoctorProfileResponse
                {
                    Success = false,
                    Message = "حدث خطأ في الخادم"
                });
            }
        }

        /// <summary>
        /// البحث عن مريض بالرقم الوطني
        /// </summary>
        [HttpPost("search-patient")]
        public async Task<ActionResult<PatientSearchResponse>> SearchPatient([FromBody] PatientSearchRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new PatientSearchResponse
                    {
                        Success = false,
                        Message = "البيانات المدخلة غير صحيحة"
                    });
                }

                //البحث حسب NationalId أو PatientCode
               var patient = await _context.Patients
                   .Include(p => p.User)
                   .Include(p => p.Allergies)
                   .Include(p => p.ChronicDiseases)
                   .Include(p => p.Surgeries)
                   .Include(p => p.MedicalRecords)
                       .ThenInclude(mr => mr.Doctor)
                   .Include(p => p.Prescriptions)
                       .ThenInclude(pr => pr.PrescriptionItems)
                   .FirstOrDefaultAsync(p => p.User.NationalId == request.Identifier
                                          || p.PatientCode == request.Identifier);

                if (patient == null)
                {
                    return NotFound(new PatientSearchResponse
                    {
                        Success = false,
                        Message = "لم يتم العثور على المريض"
                    });
                }

                var patientInfo = new PatientMedicalInfo
                {
                    Id = patient.UserId,
                    FullName = patient.FullName,
                    DateOfBirth = patient.DateOfBirth,
                    Gender = patient.Gender,
                    BloodType = patient.BloodType,
                    Weight = patient.Weight,
                    Height = patient.Height,
                    EmergencyContact = patient.EmergencyContact,
                    EmergencyPhone = patient.EmergencyPhone,
                    Allergies = patient.Allergies.Select(a => new AllergyInfo
                    {
                        Id = a.Id,
                        PatientId = a.PatientId,
                        AllergenName = a.AllergenName,
                        Reaction = a.Reaction,
                        Severity = a.Severity,
                        CreatedAt = a.CreatedAt
                    }).ToList(),
                    ChronicDiseases = patient.ChronicDiseases.Select(cd => new ChronicDiseaseInfo
                    {
                        Id = cd.Id,
                        PatientId = cd.PatientId,
                        DiseaseName = cd.DiseaseName,
                        Description = cd.Description,
                        DiagnosisDate = cd.DiagnosisDate,
                        CreatedAt = cd.CreatedAt
                    }).ToList(),
                    Surgeries = patient.Surgeries.Select(s => new SurgeryInfo
                    {
                        Id = s.Id,
                        PatientId = s.PatientId,
                        SurgeryName = s.SurgeryName,
                        Description = s.Description,
                        SurgeryDate = s.SurgeryDate,
                        Hospital = s.Hospital,
                        Surgeon = s.Surgeon,
                        CreatedAt = s.CreatedAt
                    }).ToList(),
                    MedicalRecords = patient.MedicalRecords.Select(mr => new MedicalRecordInfo
                    {
                        Id = mr.Id,
                        PatientId = mr.PatientId,
                        DoctorId = null,
                        Diagnosis = mr.Diagnosis,
                        Notes = mr.Notes,
                        Symptoms = mr.Symptoms,
                        Treatment = mr.Treatment,
                        RecordDate = mr.RecordDate,
                        CreatedAt = mr.CreatedAt,
                        DoctorName = mr.Doctor.FullName,
                        PatientName = patient.FullName
                    }).ToList(),
                    Prescriptions = patient.Prescriptions.Select(p => new PrescriptionInfo
                    {
                        Id = p.Id,
                        PatientId = p.PatientId,
                        DoctorId = p.DoctorId,
                        Diagnosis = p.Diagnosis,
                        Notes = p.Notes,
                        Status = p.Status,
                        PrescriptionDate = p.PrescriptionDate,
                        CreatedAt = p.CreatedAt,
                        DoctorName = p.Doctor.FullName,
                        PatientName = patient.FullName,
                        Items = p.PrescriptionItems.Select(pi => new PrescriptionItemInfo
                        {
                            Id = pi.Id,
                            PrescriptionId = pi.PrescriptionId,
                            MedicationName = pi.MedicationName,
                            Dosage = pi.Dosage,
                            Frequency = pi.Frequency,
                            Duration = pi.Duration,
                            Instructions = pi.Instructions,
                            Quantity = pi.Quantity,
                            IsDispensed = pi.IsDispensed,
                            CreatedAt = pi.CreatedAt
                        }).ToList()
                    }).ToList()
                };

                return Ok(new PatientSearchResponse
                {
                    Success = true,
                    Message = "تم العثور على المريض بنجاح",
                    Patient = patientInfo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for patient");
                return StatusCode(500, new PatientSearchResponse
                {
                    Success = false,
                    Message = "حدث خطأ في الخادم"
                });
            }
        }



        /// <summary>
        /// طلب الوصول للمريض عبر QR Code
        /// </summary>
        [HttpPost("request-access")]
        public async Task<ActionResult<AccessRequestResponse>> RequestAccess([FromBody] AccessRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var result = await _qrCodeService.RequestAccessAsync(request.Token, userId.Value);

                if (!result.Success)
                {
                    return BadRequest(new AccessRequestResponse
                    {
                        Success = false,
                        Message = result.Message
                    });
                }

                return Ok(new AccessRequestResponse
                {
                    Success = true,
                    Message = result.Message,
                    RequestId = result.RequestId,
                    RequiresApproval = result.RequiresApproval
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting access");
                return StatusCode(500, new AccessRequestResponse
                {
                    Success = false,
                    Message = "حدث خطأ في الخادم"
                });
            }
        }


        [HttpPost("medical-record")]
        public async Task<ActionResult<MedicalRecordResponse>> AddMedicalRecord([FromBody] MedicalRecordRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new MedicalRecordResponse
                    {
                        Success = false,
                        Message = "البيانات المدخلة غير صحيحة"
                    });
                }

                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var medicalRecord = new MedicalRecord
                {
                    PatientId = request.PatientId,
                    DoctorId = userId.Value,
                    Diagnosis = request.Diagnosis,
                    Notes = request.Notes,
                    Symptoms = request.Symptoms,
                    Treatment = request.Treatment,
                    RecordDate = request.RecordDate,
                    CreatedAt = DateTime.UtcNow
                };

                _context.MedicalRecords.Add(medicalRecord);
                await _context.SaveChangesAsync();

                //Log the action
               await LogUserAction(userId.Value, "ADD_MEDICAL_RECORD",
                   $"تم إضافة سجل طبي جديد للمريض {request.PatientId}");

                var medicalRecordInfo = new MedicalRecordInfo
                {
                    Id = medicalRecord.Id,
                    PatientId = medicalRecord.PatientId,
                    DoctorId = medicalRecord.DoctorId,
                    Diagnosis = medicalRecord.Diagnosis,
                    Notes = medicalRecord.Notes,
                    Symptoms = medicalRecord.Symptoms,
                    Treatment = medicalRecord.Treatment,
                    RecordDate = medicalRecord.RecordDate,
                    CreatedAt = medicalRecord.CreatedAt
                };

                return Ok(new MedicalRecordResponse
                {
                    Success = true,
                    Message = "تم إضافة السجل الطبي بنجاح",
                    MedicalRecord = medicalRecordInfo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding medical record");
                return StatusCode(500, new MedicalRecordResponse
                {
                    Success = false,
                    Message = "حدث خطأ في الخادم"
                });
            }
        }

        /// <summary>
        /// إضافة وصفة طبية جديدة
        /// </summary>
        /// 
        [HttpPost("AddPrescription")]
        public async Task<ActionResult<PrescriptionResponse>> AddPrescription([FromBody] PrescriptionRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new PrescriptionResponse
                    {
                        Success = false,
                        Message = "البيانات المدخلة غير صحيحة"
                    });
                }

                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                //Check for drug interactions

               var interactionRequest = new DrugInteractionCheckRequest
               {
                   PatientId = request.PatientId,
                   Medications = request.Items
                       .Select(i => i.MedicationName)
                       .ToList()
               };

                var interactionResult = await _drugInteractionService.CheckDrugInteractionsAsync(interactionRequest);

                var prescription = new Prescription
                {
                    PatientId = request.PatientId,
                    DoctorId = userId.Value,
                    Diagnosis = request.Diagnosis,
                    Notes = request.Notes,
                    Status = PrescriptionStatus.Pending,
                    PrescriptionDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Prescriptions.Add(prescription);
                await _context.SaveChangesAsync();

                //Add prescription items
                foreach (var item in request.Items)
                {
                    var prescriptionItem = new PrescriptionItem
                    {
                        PrescriptionId = prescription.Id,
                        MedicationName = item.MedicationName,
                        Dosage = item.Dosage,
                        Frequency = item.Frequency,
                        Duration = item.Duration,
                        Instructions = item.Instructions,
                        Quantity = item.Quantity,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.PrescriptionItems.Add(prescriptionItem);
                }

                await _context.SaveChangesAsync();

                //Log the action
               await LogUserAction(userId.Value, "ADD_PRESCRIPTION",
                   $"تم إضافة وصفة طبية جديدة للمريض {request.PatientId}");

                var prescriptionInfo = new PrescriptionInfo
                {
                    Id = prescription.Id,
                    PatientId = prescription.PatientId,
                    DoctorId = prescription.DoctorId,
                    Diagnosis = prescription.Diagnosis,
                    Notes = prescription.Notes,
                    Status = prescription.Status,
                    PrescriptionDate = prescription.PrescriptionDate,
                    CreatedAt = prescription.CreatedAt,
                    Items = request.Items.Select(i => new PrescriptionItemInfo
                    {
                        PrescriptionId = prescription.Id,
                        MedicationName = i.MedicationName,
                        Dosage = i.Dosage,
                        Frequency = i.Frequency,
                        Duration = i.Duration,
                        Instructions = i.Instructions,
                        Quantity = i.Quantity,
                        IsDispensed = false,
                        CreatedAt = DateTime.UtcNow
                    }).ToList()
                };

                return Ok(new PrescriptionResponse
                {
                    Success = true,
                    Message = interactionResult.HasInteractions
                        ? "تم إضافة الوصفة الطبية مع تحذيرات التفاعل الدوائي"
                        : "تم إضافة الوصفة الطبية بنجاح",
                    Prescription = prescriptionInfo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding prescription");
                return StatusCode(500, new PrescriptionResponse
                {
                    Success = false,
                    Message = "حدث خطأ في الخادم"
                });
            }
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return null;
        }

        private async Task LogUserAction(int userId, string action, string description)
        {
            try
            {
                _context.AuditLogs.Add(new AuditLog
                {
                    UserId = userId,
                    Action = action,
                    Description = description,
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging user action {Action} for user {UserId}", action, userId);
            }


        }








        /// <summary>
        /// تفعيل ملف المريض للأطباء والصيادلة
        /// </summary>
        [HttpPost("activate-patient-profile")]
        [Authorize(Roles = "Doctor,Pharmacist")]
        public async Task<ActionResult<LoginResponse>> ActivatePatientProfile()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var user = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == userId.Value);

            if (user == null) return NotFound("User not found.");

            // التحقق من عدم وجود دور المريض مسبقاً
            if (user.Roles.Any(r => r.Role == UserRole.Patient))
            {
                return BadRequest(new { Success = false, Message = "لديك ملف طبي مفعل بالفعل." });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // إنشاء سجل المريض
                var patientProfile = new Patient
                {
                    UserId = user.Id,
                    FullName = user.FullName ?? "New Patient",
                    PatientCode = "PM-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(),
                    CreatedAt = DateTime.UtcNow
                };
                _context.Patients.Add(patientProfile);

                // إضافة الدور في الجدول الوسيط
                var roleAssignment = new UserRoleAssignment
                {
                    UserId = user.Id,
                    Role = UserRole.Patient
                };
                _context.UserRoleAssignments.Add(roleAssignment);

                // أضف الدور للمجموعة في الذاكرة لضمان شموله في التوكن الجديد
                user.Roles.Add(roleAssignment);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // توليد توكن جديد يحتوي على الأدوار المحدثة
                var newAccessToken = _jwtService.GenerateAccessToken(user);

                return Ok(new LoginResponse
                {
                    Success = true,
                    Message = "تم تفعيل ملفك الطبي بنجاح! يمكنك الآن التبديل إليه كـ مريض.",
                    Token = newAccessToken,
                    User = new UserInfo
                    {
                        Id = user.Id,
                        NationalId = user.NationalId,
                        FullName = user.FullName,
                        Role = user.Role, // Legacy primary role
                        Status = user.Status
                    }
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error activating patient profile for user {UserId}", user.Id);
                return StatusCode(500, "حدث خطأ أثناء تفعيل الملف الطبي");
            }
        }




    }
}









////public async Task<ActionResult<PatientSearchResponse>> SearchPatient([FromBody] PatientSearchRequest request)
////{
////    try
////    {
////        if (!ModelState.IsValid)
////        {
////            return BadRequest(new PatientSearchResponse
////            {
////                Success = false,
////                Message = "البيانات المدخلة غير صحيحة"
////            });
////        }

////        var patient = await _context.Patients
////            .Include(p => p.User)
////            .Include(p => p.Allergies)
////            .Include(p => p.ChronicDiseases)
////            .Include(p => p.Surgeries)
////            .Include(p => p.MedicalRecords)
////                .ThenInclude(mr => mr.Doctor)
////            .Include(p => p.Prescriptions)
////                .ThenInclude(pr => pr.PrescriptionItems)
////            .FirstOrDefaultAsync(p => p.User.NationalId == request.NationalId);

////        if (patient == null)
////        {
////            return NotFound(new PatientSearchResponse
////            {
////                Success = false,
////                Message = "لم يتم العثور على المريض"
////            });
////        }

////        var patientInfo = new PatientMedicalInfo
////        {
////            Id = patient.UserId,
////            FullName = patient.FullName,
////            DateOfBirth = patient.DateOfBirth,
////            Gender = patient.Gender,
////            BloodType = patient.BloodType,
////            Weight = patient.Weight,
////            Height = patient.Height,
////            EmergencyContact = patient.EmergencyContact,
////            EmergencyPhone = patient.EmergencyPhone,
////            Allergies = patient.Allergies.Select(a => new AllergyInfo
////            {
////                Id = a.Id,
////                PatientId = a.PatientId,
////                AllergenName = a.AllergenName,
////                Reaction = a.Reaction,
////                Severity = a.Severity,
////                CreatedAt = a.CreatedAt
////            }).ToList(),
////            ChronicDiseases = patient.ChronicDiseases.Select(cd => new ChronicDiseaseInfo
////            {
////                Id = cd.Id,
////                PatientId = cd.PatientId,
////                DiseaseName = cd.DiseaseName,
////                Description = cd.Description,
////                DiagnosisDate = cd.DiagnosisDate,
////                CreatedAt = cd.CreatedAt
////            }).ToList(),
////            Surgeries = patient.Surgeries.Select(s => new SurgeryInfo
////            {
////                Id = s.Id,
////                PatientId = s.PatientId,
////                SurgeryName = s.SurgeryName,
////                Description = s.Description,
////                SurgeryDate = s.SurgeryDate,
////                Hospital = s.Hospital,
////                Surgeon = s.Surgeon,
////                CreatedAt = s.CreatedAt
////            }).ToList(),
////            MedicalRecords = patient.MedicalRecords.Select(mr => new MedicalRecordInfo
////            {
////                Id = mr.Id,
////                PatientId = mr.PatientId,
////                DoctorId = null,
////                Diagnosis = mr.Diagnosis,
////                Notes = mr.Notes,
////                Symptoms = mr.Symptoms,
////                Treatment = mr.Treatment,
////                RecordDate = mr.RecordDate,
////                CreatedAt = mr.CreatedAt,
////                DoctorName = mr.Doctor.FullName,
////                PatientName = patient.FullName
////            }).ToList(),
////            Prescriptions = patient.Prescriptions.Select(p => new PrescriptionInfo
////            {
////                Id = p.Id,
////                PatientId = p.PatientId,
////                DoctorId = p.DoctorId,
////                Diagnosis = p.Diagnosis,
////                Notes = p.Notes,
////                Status = p.Status,
////                PrescriptionDate = p.PrescriptionDate,
////                CreatedAt = p.CreatedAt,
////                DoctorName = p.Doctor.FullName,
////                PatientName = patient.FullName,
////                Items = p.PrescriptionItems.Select(pi => new PrescriptionItemInfo
////                {
////                    Id = pi.Id,
////                    PrescriptionId = pi.PrescriptionId,
////                    MedicationName = pi.MedicationName,
////                    Dosage = pi.Dosage,
////                    Frequency = pi.Frequency,
////                    Duration = pi.Duration,
////                    Instructions = pi.Instructions,
////                    Quantity = pi.Quantity,
////                    IsDispensed = pi.IsDispensed,
////                    CreatedAt = pi.CreatedAt
////                }).ToList()
////            }).ToList()
////        };

////        return Ok(new PatientSearchResponse
////        {
////            Success = true,
////            Message = "تم العثور على المريض بنجاح",
////            Patient = patientInfo
////        });
////    }
////    catch (Exception ex)
////    {
////        _logger.LogError(ex, "Error searching for patient");
////        return StatusCode(500, new PatientSearchResponse
////        {
////            Success = false,
////            Message = "حدث خطأ في الخادم"
////        });
////    }
////}