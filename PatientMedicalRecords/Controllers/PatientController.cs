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
    //[Authorize(Roles = "Patient,Doctor,Pharmacist")]
    [Authorize]
    public class PatientController : ControllerBase
    {
        private readonly MedicalRecordsDbContext _context;
        private readonly IQRCodeService _qrCodeService;
        private readonly ILogger<PatientController> _logger;

        public PatientController(
            MedicalRecordsDbContext context,
            IQRCodeService qrCodeService,
            ILogger<PatientController> logger)
        {
            _context = context;
            _qrCodeService = qrCodeService;
            _logger = logger;
        }



        [HttpPost("initialize-profile")]
        public async Task<ActionResult<InitializePatientProfileResponse>> InitializeProfile([FromBody] InitializePatientProfileRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new InitializePatientProfileResponse
                    {
                        Success = false,
                        Message = "البيانات المدخلة غير صحيحة"
                    });
                }

                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                // احضر ملف المريض
                var patient = await _context.Patients
                    .Include(p => p.Allergies)
                    .Include(p => p.ChronicDiseases)
                    .Include(p => p.Surgeries)
                    .Include(p => p.MedicalRecords)
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (patient == null)
                {
                    return NotFound(new InitializePatientProfileResponse
                    {
                        Success = false,
                        Message = "لم يتم العثور على ملف المريض"
                    });
                }

                // منع إعادة التهيئة إذا كان لديه بيانات طبية سابقة
                var alreadyInitialized = patient.Allergies.Any() || patient.ChronicDiseases.Any() || patient.Surgeries.Any() || patient.MedicalRecords.Any();
                if (alreadyInitialized)
                {
                    return Conflict(new InitializePatientProfileResponse
                    {
                        Success = false,
                        Message = "تم تهيئة الملف الطبي مسبقاً"
                    });
                }

                using var trx = await _context.Database.BeginTransactionAsync();
                try
                {
                    // تحديث بيانات التعريف الأساسية
                    patient.FullName = request.FullName;
                    patient.DateOfBirth = request.DateOfBirth;
                    patient.Gender = request.Gender;
                    patient.PhoneNumber = request.PhoneNumber;
                    patient.Email = request.Email;
                    patient.Address = request.Address;
                    patient.BloodType = request.BloodType;
                    patient.Weight = request.Weight;
                    patient.Height = request.Height;
                    patient.EmergencyContact = request.EmergencyContact;
                    patient.EmergencyPhone = request.EmergencyPhone;
                    patient.UpdatedAt = DateTime.UtcNow;

                    // Allergies
                    foreach (var a in request.Allergies)
                    {
                        _context.Allergies.Add(new Allergy
                        {
                            PatientId = patient.UserId,
                            AllergenName = a.AllergenName,
                            Reaction = a.Reaction,
                            Severity = a.Severity,
                            CreatedAt = DateTime.UtcNow
                        });
                    }

                    // Chronic Diseases
                    foreach (var d in request.ChronicDiseases)
                    {
                        _context.ChronicDiseases.Add(new ChronicDisease
                        {
                            PatientId = patient.UserId,
                            DiseaseName = d.DiseaseName,
                            Description = d.Description,
                            DiagnosisDate = d.DiagnosisDate,
                            CreatedAt = DateTime.UtcNow
                        });
                    }

                    // Surgeries
                    foreach (var s in request.Surgeries)
                    {
                        _context.Surgeries.Add(new Surgery
                        {
                            PatientId = patient.UserId,
                            SurgeryName = s.SurgeryName,
                            Description = s.Description,
                            SurgeryDate = s.SurgeryDate,
                            Hospital = s.Hospital,
                            Surgeon = s.Surgeon,
                            CreatedAt = DateTime.UtcNow
                        });
                    }

                    // Create an initial medical record to document current medications and notes
                    var firstDoctor = await _context.Doctors.FirstOrDefaultAsync();

                    if (firstDoctor == null)
                    {
                        return BadRequest("لا يوجد أي دكتور مسجل في النظام.");
                    }
                    var initialRecord = new MedicalRecord
                    {
                        PatientId = patient.UserId,
                        DoctorId = firstDoctor.UserId, // self-initiated; could be 0 or special value if needed
                        Diagnosis = "Initial Medical Profile",
                        Notes = request.Notes,
                        Symptoms = null,
                        Treatment = null,
                        RecordDate = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.MedicalRecords.Add(initialRecord);
                    await _context.SaveChangesAsync();

                    // Represent current medications in a technical prescription to keep auditability

                    var firstDoctor1 = await _context.Doctors.FirstOrDefaultAsync();

                    if (firstDoctor1 == null)
                    {
                        return BadRequest("لا يوجد أي دكتور مسجل في النظام.");
                    }
                    if (request.CurrentMedications.Any())
                    {
                        var initPrescription = new Prescription
                        {
                            PatientId = patient.UserId,
                            DoctorId = firstDoctor.UserId,
                            Diagnosis = "Current Medications (Patient Provided)",
                            Notes = "Initialized from patient self profile",
                            Status = PrescriptionStatus.Dispensed,
                            PrescriptionDate = DateTime.UtcNow,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.Prescriptions.Add(initPrescription);
                        await _context.SaveChangesAsync();

                        foreach (var m in request.CurrentMedications)
                        {
                            _context.PrescriptionItems.Add(new PrescriptionItem
                            {
                                PrescriptionId = initPrescription.Id,
                                MedicationName = m.MedicationName,
                                Dosage = m.Dosage,
                                Frequency = m.Frequency,
                                Duration = m.Duration,
                                Instructions = m.Instructions,
                                Quantity = 0,
                                IsDispensed = true,
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                    }

                    await _context.SaveChangesAsync();
                    await trx.CommitAsync();

                    await LogUserAction(userId.Value, "INITIALIZE_PROFILE", "تم تهيئة الملف الطبي لأول مرة");

                    return Ok(new InitializePatientProfileResponse
                    {
                        Success = true,
                        Message = "تم تهيئة الملف الطبي بنجاح",
                        PatientId = patient.UserId
                    });
                }
                catch (Exception exIn)
                {
                    await trx.RollbackAsync();
                    _logger.LogError(exIn, "Error initializing patient profile");
                    return StatusCode(500, new InitializePatientProfileResponse
                    {
                        Success = false,
                        Message = "حدث خطأ أثناء تهيئة الملف الطبي"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in initialize profile endpoint");
                return StatusCode(500, new InitializePatientProfileResponse
                {
                    Success = false,
                    Message = "حدث خطأ في الخادم"
                });
            }
        }



        /// <summary>
        /// الحصول على ملف المريض الشخصي
        /// </summary>
        [HttpGet("profile")]
        public async Task<ActionResult<PatientProfileResponse>> GetProfile()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var patient = await _context.Patients
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (patient == null)
                {
                    return NotFound(new PatientProfileResponse
                    {
                        Success = false,
                        Message = "لم يتم العثور على ملف المريض"
                    });
                }

                var patientInfo = new PatientInfo
                {
                    Id = patient.UserId,
                    UserId = patient.UserId,
                    FullName = patient.FullName,
                    DateOfBirth = patient.DateOfBirth,
                    Gender = patient.Gender,
                    PhoneNumber = patient.PhoneNumber,
                    Email = patient.Email,
                    Address = patient.Address,
                    BloodType = patient.BloodType,
                    Weight = patient.Weight,
                    Height = patient.Height,
                    EmergencyContact = patient.EmergencyContact,
                    EmergencyPhone = patient.EmergencyPhone,
                    CreatedAt = patient.CreatedAt,
                    UpdatedAt = patient.UpdatedAt
                };

                return Ok(new PatientProfileResponse
                {
                    Success = true,
                    Message = "تم جلب ملف المريض بنجاح",
                    Patient = patientInfo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting patient profile");
                return StatusCode(500, new PatientProfileResponse
                {
                    Success = false,
                    Message = "حدث خطأ في الخادم"
                });
            }
        }

        /// <summary>
        /// تحديث ملف المريض الشخصي
        /// </summary>
        [HttpPut("profile")]
        public async Task<ActionResult<PatientProfileResponse>> UpdateProfile([FromBody] PatientProfileRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new PatientProfileResponse
                    {
                        Success = false,
                        Message = "البيانات المدخلة غير صحيحة"
                    });
                }

                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var patient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (patient == null)
                {
                    return NotFound(new PatientProfileResponse
                    {
                        Success = false,
                        Message = "لم يتم العثور على ملف المريض"
                    });
                }

                // Update patient information
                patient.FullName = request.FullName;
                patient.DateOfBirth = request.DateOfBirth;
                patient.Gender = request.Gender;
                patient.PhoneNumber = request.PhoneNumber;
                patient.Email = request.Email;
                patient.Address = request.Address;
                patient.BloodType = request.BloodType;
                patient.Weight = request.Weight;
                patient.Height = request.Height;
                patient.EmergencyContact = request.EmergencyContact;
                patient.EmergencyPhone = request.EmergencyPhone;
                patient.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Log the update
                await LogUserAction(userId.Value, "UPDATE_PROFILE", "تم تحديث ملف المريض الشخصي");

                var patientInfo = new PatientInfo
                {
                    Id = patient.UserId,
                    UserId = patient.UserId,
                    FullName = patient.FullName,
                    DateOfBirth = patient.DateOfBirth,
                    Gender = patient.Gender,
                    PhoneNumber = patient.PhoneNumber,
                    Email = patient.Email,
                    Address = patient.Address,
                    BloodType = patient.BloodType,
                    Weight = patient.Weight,
                    Height = patient.Height,
                    EmergencyContact = patient.EmergencyContact,
                    EmergencyPhone = patient.EmergencyPhone,
                    CreatedAt = patient.CreatedAt,
                    UpdatedAt = patient.UpdatedAt
                };

                return Ok(new PatientProfileResponse
                {
                    Success = true,
                    Message = "تم تحديث ملف المريض بنجاح",
                    Patient = patientInfo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating patient profile");
                return StatusCode(500, new PatientProfileResponse
                {
                    Success = false,
                    Message = "حدث خطأ في الخادم"
                });
            }
        }

        /// <summary>
        /// توليد رمز QR
        /// </summary>
        [HttpPost("generate-qr")]
        public async Task<ActionResult<QRCodeResponse>> GenerateQRCode()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var request = new QRCodeRequest { UserId = userId.Value };
                var result = await _qrCodeService.GenerateQRCodeAsync(request);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code");
                return StatusCode(500, new QRCodeResponse
                {
                    Success = false,
                    Message = "حدث خطأ في الخادم"
                });
            }
        }

        /// <summary>
        /// الحصول على شاشة الطوارئ
        /// </summary>
        [HttpGet("emergency-screen")]
        public async Task<ActionResult<EmergencyScreenResponse>> GetEmergencyScreen()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var patient = await _context.Patients
                    .Include(p => p.Allergies)
                    .Include(p => p.ChronicDiseases)
                    .Include(p => p.Prescriptions)
                        .ThenInclude(pr => pr.PrescriptionItems)
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (patient == null)
                {
                    return NotFound(new EmergencyScreenResponse
                    {
                        Success = false,
                        Message = "لم يتم العثور على ملف المريض"
                    });
                }

                // Generate QR code for emergency access
                var qrRequest = new QRCodeRequest { UserId = userId.Value };
                var qrResult = await _qrCodeService.GenerateQRCodeAsync(qrRequest);

                var emergencyInfo = new EmergencyInfo
                {
                    FullName = patient.FullName,
                    BloodType = patient.BloodType,
                    Allergies = patient.Allergies.Select(a => a.AllergenName).ToList(),
                    ChronicDiseases = patient.ChronicDiseases.Select(cd => cd.DiseaseName).ToList(),
                    CurrentMedications = patient.Prescriptions
                        .Where(p => p.Status == PrescriptionStatus.Dispensed)
                        .SelectMany(p => p.PrescriptionItems.Where(pi => pi.IsDispensed))
                        .Select(pi => pi.MedicationName)
                        .Distinct()
                        .ToList(),
                    EmergencyContact = patient.EmergencyContact,
                    EmergencyPhone = patient.EmergencyPhone,
                    QRCodeUrl = qrResult.QRCodeUrl
                };

                return Ok(new EmergencyScreenResponse
                {
                    Success = true,
                    Message = "تم جلب معلومات الطوارئ بنجاح",
                    EmergencyInfo = emergencyInfo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting emergency screen");
                return StatusCode(500, new EmergencyScreenResponse
                {
                    Success = false,
                    Message = "حدث خطأ في الخادم"
                });
            }
        }

        /// <summary>
        /// الحصول على السجلات الطبية
        /// </summary>
        [HttpGet("medical-records")]
        public async Task<ActionResult<List<MedicalRecordInfo>>> GetMedicalRecords()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var patient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (patient == null)
                {
                    return NotFound();
                }

                var medicalRecords = await _context.MedicalRecords
                    .Include(mr => mr.Doctor)
                    .Where(mr => mr.PatientId == patient.UserId)
                    .OrderByDescending(mr => mr.RecordDate)
                    .Select(mr => new MedicalRecordInfo
                    {
                        Id = mr.Id,
                        PatientId = mr.Id,
                        DoctorId = mr.DoctorId,
                        Diagnosis = mr.Diagnosis,
                        Notes = mr.Notes,
                        Symptoms = mr.Symptoms,
                        Treatment = mr.Treatment,
                        RecordDate = mr.RecordDate,
                        CreatedAt = mr.CreatedAt,
                        DoctorName = mr.Doctor.FullName,
                        PatientName = patient.FullName
                    })
                    .ToListAsync();

                return Ok(medicalRecords);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting medical records");
                return StatusCode(500, "حدث خطأ في الخادم");
            }
        }

        /// <summary>
        /// الحصول على الوصفات الطبية
        /// </summary>
        [HttpGet("prescriptions")]
        public async Task<ActionResult<List<PrescriptionInfo>>> GetPrescriptions()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var patient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (patient == null)
                {
                    return NotFound();
                }

                var prescriptions = await _context.Prescriptions
                    .Include(p => p.Doctor)
                    .Include(p => p.PrescriptionItems)
                    .Where(p => p.PatientId == patient.UserId)
                    .OrderByDescending(p => p.PrescriptionDate)
                    .Select(p => new PrescriptionInfo
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
                    })
                    .ToListAsync();

                return Ok(prescriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prescriptions");
                return StatusCode(500, "حدث خطأ في الخادم");
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
    }
}
