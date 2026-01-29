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
    public class PharmacistController : ControllerBase
    {
        private readonly MedicalRecordsDbContext _context;
        private readonly IQRCodeService _qrCodeService;
        private readonly IDrugInteractionService _drugInteractionService;
        private readonly ILogger<PharmacistController> _logger;

        public PharmacistController(
            MedicalRecordsDbContext context,
            IQRCodeService qrCodeService,
            IDrugInteractionService drugInteractionService,
            ILogger<PharmacistController> logger)
        {
            _context = context;
            _qrCodeService = qrCodeService;
            _drugInteractionService = drugInteractionService;
            _logger = logger;
        }


        /// <summary>
        /// الحصول على ملف الصيدلي الشخصي
        /// </summary>
        [HttpGet("profile")]
        public async Task<ActionResult<PharmacistProfileResponse>> GetProfile()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var pharmacist = await _context.Pharmacists
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (pharmacist == null)
                {
                    return NotFound(new PharmacistProfileResponse
                    {
                        Success = false,
                        Message = "لم يتم العثور على ملف الصيدلي"
                    });
                }

                var pharmacistInfo = new PharmacistInfo
                {
                    Id = pharmacist.UserId,
                    UserId = pharmacist.UserId,
                    FullName = pharmacist.FullName,
                    LicenseNumber = pharmacist.LicenseNumber,
                    PharmacyName = pharmacist.PharmacyName,
                    PhoneNumber = pharmacist.PhoneNumber,
                    Email = pharmacist.Email,
                    CreatedAt = pharmacist.CreatedAt,
                    UpdatedAt = pharmacist.UpdatedAt
                };

                return Ok(new PharmacistProfileResponse
                {
                    Success = true,
                    Message = "تم جلب ملف الصيدلي بنجاح",
                    Pharmacist = pharmacistInfo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pharmacist profile");
                return StatusCode(500, new PharmacistProfileResponse
                {
                    Success = false,
                    Message = "حدث خطأ في الخادم"
                });
            }
        }

        /// <summary>
        /// تحديث ملف الصيدلي الشخصي
        /// </summary>
        [HttpPut("profile")]
        public async Task<ActionResult<PharmacistProfileResponse>> UpdateProfile([FromBody] PharmacistProfileRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new PharmacistProfileResponse
                    {
                        Success = false,
                        Message = "البيانات المدخلة غير صحيحة"
                    });
                }

                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var pharmacist = await _context.Pharmacists
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (pharmacist == null)
                {
                    return NotFound(new PharmacistProfileResponse
                    {
                        Success = false,
                        Message = "لم يتم العثور على ملف الصيدلي"
                    });
                }

                // Update pharmacist information
                pharmacist.FullName = request.FullName;
                pharmacist.LicenseNumber = request.LicenseNumber;
                pharmacist.PharmacyName = request.PharmacyName;
                pharmacist.PhoneNumber = request.PhoneNumber;
                pharmacist.Email = request.Email;
                pharmacist.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Log the update
                await LogUserAction(userId.Value, "UPDATE_PROFILE", "تم تحديث ملف الصيدلي الشخصي");

                var pharmacistInfo = new PharmacistInfo
                {
                    Id = pharmacist.UserId,
                    UserId = pharmacist.UserId,
                    FullName = pharmacist.FullName,
                    LicenseNumber = pharmacist.LicenseNumber,
                    PharmacyName = pharmacist.PharmacyName,
                    PhoneNumber = pharmacist.PhoneNumber,
                    Email = pharmacist.Email,
                    CreatedAt = pharmacist.CreatedAt,
                    UpdatedAt = pharmacist.UpdatedAt
                };

                return Ok(new PharmacistProfileResponse
                {
                    Success = true,
                    Message = "تم تحديث ملف الصيدلي بنجاح",
                    Pharmacist = pharmacistInfo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating pharmacist profile");
                return StatusCode(500, new PharmacistProfileResponse
                {
                    Success = false,
                    Message = "حدث خطأ في الخادم"
                });
            }
        }

        /// <summary>
        /// البحث عن وصفة طبية بالرقم الوطني
        /// </summary>
        [HttpPost("search-prescription")]
        public async Task<ActionResult<PrescriptionSearchResponse>> SearchPrescription([FromBody] PrescriptionSearchRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new PrescriptionSearchResponse
                    {
                        Success = false,
                        Message = "البيانات المدخلة غير صحيحة"
                    });
                }

                var patient = await _context.Patients
                    .Include(p => p.User)
                    .Include(p => p.Prescriptions)
                    .FirstOrDefaultAsync(p => p.User.NationalId == request.Identifier
                    || p.PatientCode == request.Identifier);

                if (patient == null)
                {
                    return NotFound(new PrescriptionSearchResponse
                    {
                        Success = false,
                        Message = "لم يتم العثور على المريض"
                    });
                }
                var prescriptions = await _context.Prescriptions
    .Include(p => p.Doctor)
    .Include(p => p.PrescriptionItems)
    .Where(p => p.PatientId == patient.UserId && p.Status == PrescriptionStatus.Pending)
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


                return Ok(new PrescriptionSearchResponse
                {
                    Success = true,
                    Message = "تم العثور على الوصفات الطبية",
                    Prescriptions = prescriptions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for prescriptions");
                return StatusCode(500, new PrescriptionSearchResponse
                {
                    Success = false,
                    Message = "حدث خطأ في الخادم"
                });
            }
        }

        /// <summary>
        /// فحص التفاعلات الدوائية
        /// </summary>
        [HttpPost("check-drug-interactions")]
        public async Task<ActionResult<DrugInteractionCheckResponse>> CheckDrugInteractions([FromBody] DrugInteractionCheckRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new DrugInteractionCheckResponse
                    {
                        Success = false,
                        Message = "البيانات المدخلة غير صحيحة"
                    });
                }

                var result = await _drugInteractionService.CheckDrugInteractionsAsync(request);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking drug interactions");
                return StatusCode(500, new DrugInteractionCheckResponse
                {
                    Success = false,
                    Message = "حدث خطأ في الخادم"
                });
            }
        }

        /// <summary>
        /// صرف الوصفة الطبية
        /// </summary>
        [HttpPost("dispense-prescription")]
        public async Task<ActionResult<PrescriptionDispenseResponse>> DispensePrescription([FromBody] PrescriptionDispenseRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new PrescriptionDispenseResponse
                    { Success = false, Message = "البيانات المدخلة غير صحيحة" });

                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var prescription = await _context.Prescriptions
                    .Include(p => p.PrescriptionItems)
                    .FirstOrDefaultAsync(p => p.Id == request.PrescriptionId);

                if (prescription == null)
                    return NotFound(new PrescriptionDispenseResponse
                    { Success = false, Message = "لم يتم العثور على الوصفة الطبية" });

                // إنشاء قاموس للمطابقة السريعة وللحماية من IDs غير صحيحة
                var itemMap = prescription.PrescriptionItems.ToDictionary(pi => pi.Id);

                // تجميع أسماء الأدوية بطريقة آمنة
                var medications = request.Items
                    .Where(i => i.Dispensed && itemMap.ContainsKey(i.PrescriptionItemId))
                    .Select(i => itemMap[i.PrescriptionItemId].MedicationName)
                    .ToList();

                // استخدام PatientId الصحيح (وليس prescription.Id)
                var interactionRequest = new DrugInteractionCheckRequest
                {
                    PatientId = prescription.PatientId, // <-- تأكد أن الحقل موجود في نموذجك
                    Medications = prescription.PrescriptionItems
                        .Where(pi => request.Items.Any(i => i.PrescriptionItemId == pi.Id && i.Dispensed))
                        .Select(pi => pi.MedicationName)
                        .ToList()
                };

                var interactionResult = await _drugInteractionService.CheckDrugInteractionsAsync(interactionRequest);

                // تحديث عناصر الوصفة بأمان
                foreach (var item in request.Items)
                {
                    if (itemMap.TryGetValue(item.PrescriptionItemId, out var pi))
                    {
                        pi.IsDispensed = item.Dispensed;
                    }
                    // إن لم توجد المطابقة، تجاهل أو سجل تحذيراً بحسب سياساتك
                }

                // استخدم معاملة لضمان الاتساق
                using var tx = await _context.Database.BeginTransactionAsync();
                try
                {
                    var dispense = new PrescriptionDispense
                    {
                        PrescriptionId = request.PrescriptionId,
                        PharmacistId = userId.Value,
                        Notes = request.Notes,
                        DispenseDate = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.PrescriptionDispenses.Add(dispense);

                    var dispensedItems = prescription.PrescriptionItems.Count(pi => pi.IsDispensed);
                    var totalItems = prescription.PrescriptionItems.Count;

                    if (dispensedItems == totalItems)
                        prescription.Status = PrescriptionStatus.Dispensed;
                    else if (dispensedItems > 0)
                        prescription.Status = PrescriptionStatus.PartiallyDispensed;

                    await _context.SaveChangesAsync();
                    await tx.CommitAsync();

                    await LogUserAction(userId.Value, "DISPENSE_PRESCRIPTION", $"تم صرف الوصفة الطبية {request.PrescriptionId}");
                }
                catch (Exception ex)
                {
                    await tx.RollbackAsync();
                    _logger.LogError(ex, "Error dispensing prescription transaction");
                    throw; // أو عُدّ خطأ مناسب
                }

                // إعداد DTO الاستجابة (كما في كودك الأصلي)
                var dispenseInfo = new PrescriptionDispenseInfo
                {
                    // املأ الحقول كما تريد؛ تأكد أن `dispense.Id` متاح بعد الحفظ إن لزم
                    Items = prescription.PrescriptionItems.Select(pi => new DispensedItemInfo
                    {
                        Id = pi.Id,
                        PrescriptionItemId = pi.Id,
                        MedicationName = pi.MedicationName,
                        Frequency = pi.Frequency,
                        Duration = pi.Duration,
                        Instructions = pi.Instructions,
                        Dosage = pi.Dosage,
                        Quantity = pi.Quantity,
                        IsDispensed = pi.IsDispensed,
                        DispenseNotes = request.Items.FirstOrDefault(i => i.PrescriptionItemId == pi.Id)?.Notes
                    }).ToList()
                };

                return Ok(new PrescriptionDispenseResponse
                {
                    Success = true,
                    Message = interactionResult.HasInteractions ? "تم صرف الوصفة مع تحذيرات التفاعل الدوائي" : "تم صرف الوصفة بنجاح",
                    Dispense = dispenseInfo,
                    Warnings = interactionResult.Warnings
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dispensing prescription");
                return StatusCode(500, new PrescriptionDispenseResponse
                {
                    Success = false,
                    Message = "حدث خطأ في الخادم"
                });
            }
        }



        [HttpPost("create")]
        public async Task<ActionResult<ServiceResult>> CreatePrescription([FromBody] PrescriptionCreateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ServiceResult.Fail("البيانات المدخلة غير صحيحة"));

            var result = await _drugInteractionService.CreatePrescriptionAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }


        /// <summary>
        /// تحديث حالة الوصفة الطبية
        /// </summary>
        [HttpPut("prescription-status")]
        public async Task<ActionResult<PrescriptionStatusUpdateResponse>> UpdatePrescriptionStatus([FromBody] PrescriptionStatusUpdateRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new PrescriptionStatusUpdateResponse
                    {
                        Success = false,
                        Message = "البيانات المدخلة غير صحيحة"
                    });
                }

                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var prescription = await _context.Prescriptions
                    .FirstOrDefaultAsync(p => p.Id == request.PrescriptionId);

                if (prescription == null)
                {
                    return NotFound(new PrescriptionStatusUpdateResponse
                    {
                        Success = false,
                        Message = "لم يتم العثور على الوصفة الطبية"
                    });
                }

                prescription.Status = request.Status;
                await _context.SaveChangesAsync();

                // Log the action
                await LogUserAction(userId.Value, "UPDATE_PRESCRIPTION_STATUS",
                    $"تم تحديث حالة الوصفة الطبية {request.PrescriptionId} إلى {request.Status}");

                return Ok(new PrescriptionStatusUpdateResponse
                {
                    Success = true,
                    Message = "تم تحديث حالة الوصفة الطبية بنجاح",
                    NewStatus = request.Status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating prescription status");
                return StatusCode(500, new PrescriptionStatusUpdateResponse
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
    }
}
















////public async Task<ActionResult<PrescriptionDispenseResponse>> DispensePrescription([FromBody] PrescriptionDispenseRequest request)
////{
////    try
////    {
////        if (!ModelState.IsValid)
////        {
////            return BadRequest(new PrescriptionDispenseResponse
////            {
////                Success = false,
////                Message = "البيانات المدخلة غير صحيحة"
////            });
////        }

////        var userId = GetCurrentUserId();
////        if (userId == null) return Unauthorized();

////        var prescription = await _context.Prescriptions
////            .Include(p => p.PrescriptionItems)
////            .FirstOrDefaultAsync(p => p.Id == request.PrescriptionId);

////        if (prescription == null)
////        {
////            return NotFound(new PrescriptionDispenseResponse
////            {
////                Success = false,
////                Message = "لم يتم العثور على الوصفة الطبية"
////            });
////        }

////        // Check for drug interactions
////        var medications = request.Items
////            .Where(i => i.Dispensed)
////            .Select(i => prescription.PrescriptionItems.First(pi => pi.Id == i.PrescriptionItemId).MedicationName)
////            .ToList();

////        var interactionRequest = new DrugInteractionCheckRequest
////        {
////            PatientId = prescription.Id,
////            Medications = medications
////        };

////        var interactionResult = await _drugInteractionService.CheckDrugInteractionsAsync(interactionRequest);

////        // Update prescription items
////        foreach (var item in request.Items)
////        {
////            var prescriptionItem = prescription.PrescriptionItems.FirstOrDefault(pi => pi.Id == item.PrescriptionItemId);
////            if (prescriptionItem != null)
////            {
////                prescriptionItem.IsDispensed = item.Dispensed;
////            }
////        }

////        // Create dispense record
////        var dispense = new PrescriptionDispense
////        {
////            PrescriptionId = request.PrescriptionId,
////            PharmacistId = userId.Value,
////            Notes = request.Notes,
////            DispenseDate = DateTime.UtcNow,
////            CreatedAt = DateTime.UtcNow
////        };

////        _context.PrescriptionDispenses.Add(dispense);

////        // Update prescription status
////        var dispensedItems = prescription.PrescriptionItems.Count(pi => pi.IsDispensed);
////        var totalItems = prescription.PrescriptionItems.Count;

////        if (dispensedItems == totalItems)
////        {
////            prescription.Status = PrescriptionStatus.Dispensed;
////        }
////        else if (dispensedItems > 0)
////        {
////            prescription.Status = PrescriptionStatus.PartiallyDispensed;
////        }

////        await _context.SaveChangesAsync();

////        // Log the action
////        await LogUserAction(userId.Value, "DISPENSE_PRESCRIPTION", 
////            $"تم صرف الوصفة الطبية {request.PrescriptionId}");

////        var dispenseInfo = new PrescriptionDispenseInfo
////        {
////            Id = dispense.Id,
////            PrescriptionId = dispense.PrescriptionId,
////            PharmacistId = dispense.PharmacistId,
////            Notes = dispense.Notes,
////            DispenseDate = dispense.DispenseDate,
////            CreatedAt = dispense.CreatedAt,
////            Items = prescription.PrescriptionItems.Select(pi => new DispensedItemInfo
////            {
////                Id = pi.Id,
////                PrescriptionItemId = pi.Id,
////                MedicationName = pi.MedicationName,
////                Dosage = pi.Dosage,
////                Frequency = pi.Frequency,
////                Duration = pi.Duration,
////                Instructions = pi.Instructions,
////                Quantity = pi.Quantity,
////                IsDispensed = pi.IsDispensed,
////                DispenseNotes = request.Items.FirstOrDefault(i => i.PrescriptionItemId == pi.Id)?.Notes
////            }).ToList()
////        };

////        return Ok(new PrescriptionDispenseResponse
////        {
////            Success = true,
////            Message = interactionResult.HasInteractions 
////                ? "تم صرف الوصفة مع تحذيرات التفاعل الدوائي" 
////                : "تم صرف الوصفة بنجاح",
////            Dispense = dispenseInfo,
////            Warnings = interactionResult.Warnings
////        });
////    }
////    catch (Exception ex)
////    {
////        _logger.LogError(ex, "Error dispensing prescription");
////        return StatusCode(500, new PrescriptionDispenseResponse
////        {
////            Success = false,
////            Message = "حدث خطأ في الخادم"
////        });
////    }
////}


///// <summary>
///// الحصول على اقتراحات الأدوية
///// </summary>
////[HttpGet("medication-suggestions")]
////        public async Task<ActionResult<List<string>>> GetMedicationSuggestions([FromQuery] string partialName)
////        {
////            try
////            {
////                if (string.IsNullOrWhiteSpace(partialName))
////                {
////                    return BadRequest("اسم الدواء مطلوب");
////                }

////                var suggestions = await _drugInteractionService.GetMedicationSuggestionsAsync(partialName);
////                return Ok(suggestions);
////            }
////            catch (Exception ex)
////            {
////                _logger.LogError(ex, "Error getting medication suggestions");
////                return StatusCode(500, "حدث خطأ في الخادم");
////            }
////        }