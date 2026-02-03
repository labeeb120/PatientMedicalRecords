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
    [Authorize(Roles = "Pharmacist")]
    public class PharmacistController : ControllerBase
    {
        private readonly MedicalRecordsDbContext _context;
        private readonly IDrugInteractionService _drugInteractionService;
        private readonly ILogger<PharmacistController> _logger;
        private readonly IJwtService _jwtService;

        public PharmacistController(
            MedicalRecordsDbContext context,
            IDrugInteractionService drugInteractionService,
            ILogger<PharmacistController> logger,
            IJwtService jwtService)
        {
            _context = context;
            _drugInteractionService = drugInteractionService;
            _logger = logger;
            _jwtService = jwtService;
        }

        /// <summary>
        /// الحصول على إحصائيات لوحة تحكم الصيدلي
        /// </summary>
        [HttpGet("dashboard-stats")]
        public async Task<ActionResult<PharmacistDashboardStats>> GetDashboardStats()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var pharmacist = await _context.Pharmacists.FirstOrDefaultAsync(p => p.UserId == userId.Value);
                if (pharmacist == null) return NotFound("Pharmacist profile not found.");

                var stats = new PharmacistDashboardStats
                {
                    TotalDispensedPrescriptions = await _context.PrescriptionDispenses
                        .CountAsync(d => d.PharmacistId == pharmacist.UserId),

                    PendingPrescriptions = await _context.Prescriptions
                        .CountAsync(p => p.Status == PrescriptionStatus.Pending),

                    RecentDispenses = await _context.PrescriptionDispenses
                        .Include(d => d.Prescription)
                            .ThenInclude(p => p.Patient)
                        .Where(d => d.PharmacistId == pharmacist.UserId)
                        .OrderByDescending(d => d.DispenseDate)
                        .Take(5)
                        .Select(d => new RecentDispenseInfo
                        {
                            PrescriptionId = d.PrescriptionId,
                            PatientName = d.Prescription.Patient.FullName,
                            DispenseDate = d.DispenseDate,
                            Status = d.Prescription.Status
                        })
                        .ToListAsync()
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pharmacist dashboard stats");
                return StatusCode(500, "حدث خطأ في الخادم");
            }
        }

        /// <summary>
        /// البحث عن وصفة طبية لمريض
        /// </summary>
        [HttpGet("search-prescription")]
        public async Task<ActionResult<PrescriptionSearchResponse>> SearchPrescription([FromQuery] string identifier)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(identifier))
                {
                    return BadRequest(new PrescriptionSearchResponse { Success = false, Message = "المعرف مطلوب" });
                }

                var patient = await _context.Patients
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.User.NationalId == identifier || p.PatientCode == identifier);

                if (patient == null)
                {
                    return NotFound(new PrescriptionSearchResponse { Success = false, Message = "لم يتم العثور على المريض" });
                }

                var prescriptions = await _context.Prescriptions
                    .Include(p => p.Doctor)
                    .Include(p => p.PrescriptionItems)
                    .Where(p => p.PatientId == patient.UserId)
                    .OrderByDescending(p => p.PrescriptionDate)
                    .ToListAsync();

                var prescriptionInfos = prescriptions.Select(p => new PrescriptionInfo
                {
                    Id = p.Id,
                    PatientId = p.PatientId,
                    DoctorId = p.DoctorId,
                    Diagnosis = p.Diagnosis,
                    Notes = p.Notes,
                    Status = p.Status,
                    PrescriptionDate = p.PrescriptionDate,
                    CreatedAt = p.CreatedAt,
                    DoctorName = p.Doctor?.FullName ?? "N/A",
                    PatientName = patient.FullName,
                    Items = p.PrescriptionItems.Select(pi => new PrescriptionItemInfo
                    {
                        Id = pi.Id,
                        PrescriptionId = pi.PrescriptionId,
                        DrugId = pi.DrugId,
                        MedicationName = pi.MedicationName,
                        Dosage = pi.Dosage,
                        Frequency = pi.Frequency,
                        Duration = pi.Duration,
                        Instructions = pi.Instructions,
                        Quantity = pi.Quantity,
                        IsDispensed = pi.IsDispensed,
                        CreatedAt = pi.CreatedAt
                    }).ToList()
                }).ToList();

                return Ok(new PrescriptionSearchResponse
                {
                    Success = true,
                    Message = "تم العثور على الوصفات بنجاح",
                    Prescriptions = prescriptionInfos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for prescriptions");
                return StatusCode(500, new PrescriptionSearchResponse { Success = false, Message = "حدث خطأ في الخادم" });
            }
        }

        /// <summary>
        /// فحص التفاعلات الدوائية للوصفة الطبية بشكل آلي
        /// </summary>
        [HttpPost("check-interactions/{prescriptionId}")]
        public async Task<ActionResult<DrugInteractionCheckResponse>> CheckPrescriptionInteractions(int prescriptionId)
        {
            try
            {
                var prescription = await _context.Prescriptions
                    .Include(p => p.PrescriptionItems)
                    .FirstOrDefaultAsync(p => p.Id == prescriptionId);

                if (prescription == null)
                {
                    return NotFound(new DrugInteractionCheckResponse { Success = false, Message = "الوصفة غير موجودة" });
                }

                var request = new DrugInteractionCheckRequest
                {
                    PatientId = prescription.PatientId,
                    PrescriptionId = prescription.Id,
                    DrugIds = prescription.PrescriptionItems.Select(pi => pi.DrugId).ToList(),
                    Medications = prescription.PrescriptionItems.Select(pi => pi.MedicationName).ToList()
                };

                var result = await _drugInteractionService.CheckDrugInteractionsAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking interactions for prescription {PrescriptionId}", prescriptionId);
                return StatusCode(500, new DrugInteractionCheckResponse { Success = false, Message = "حدث خطأ أثناء فحص التفاعلات" });
            }
        }

        /// <summary>
        /// صرف الوصفة الطبية (صرف كلي أو جزئي لكل صنف)
        /// </summary>
        [HttpPost("dispense-prescription")]
        public async Task<ActionResult<PrescriptionDispenseResponse>> DispensePrescription([FromBody] PrescriptionDispenseRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new PrescriptionDispenseResponse { Success = false, Message = "البيانات المدخلة غير صحيحة" });
                }

                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var pharmacist = await _context.Pharmacists.FirstOrDefaultAsync(p => p.UserId == userId.Value);
                if (pharmacist == null) return NotFound(new PrescriptionDispenseResponse { Success = false, Message = "الصيدلي غير موجود" });

                var prescription = await _context.Prescriptions
                    .Include(p => p.PrescriptionItems)
                    .FirstOrDefaultAsync(p => p.Id == request.PrescriptionId);

                if (prescription == null)
                {
                    return NotFound(new PrescriptionDispenseResponse { Success = false, Message = "لم يتم العثور على الوصفة الطبية" });
                }

                // فحص التفاعلات للأصناف التي سيتم صرفها (كإجراء أمان إضافي)
                var itemsToDispense = request.Items.Where(i => i.Dispensed).ToList();
                var interactionResult = new DrugInteractionCheckResponse { HasInteractions = false };

                if (itemsToDispense.Any())
                {
                    var drugIds = prescription.PrescriptionItems
                        .Where(pi => itemsToDispense.Any(id => id.PrescriptionItemId == pi.Id))
                        .Select(pi => pi.DrugId).ToList();

                    interactionResult = await _drugInteractionService.CheckDrugInteractionsAsync(new DrugInteractionCheckRequest
                    {
                        PatientId = prescription.PatientId,
                        DrugIds = drugIds
                    });
                }

                // تحديث حالة الأصناف
                foreach (var itemRequest in request.Items)
                {
                    var item = prescription.PrescriptionItems.FirstOrDefault(pi => pi.Id == itemRequest.PrescriptionItemId);
                    if (item != null)
                    {
                        item.IsDispensed = itemRequest.Dispensed;
                    }
                }

                // إنشاء سجل الصرف
                var dispense = new PrescriptionDispense
                {
                    PrescriptionId = request.PrescriptionId,
                    PharmacistId = pharmacist.UserId,
                    Notes = request.Notes,
                    DispenseDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                _context.PrescriptionDispenses.Add(dispense);

                // تحديث حالة الوصفة بناءً على الأصناف المصروفة
                var dispensedCount = prescription.PrescriptionItems.Count(pi => pi.IsDispensed);
                var totalCount = prescription.PrescriptionItems.Count;

                if (dispensedCount == totalCount)
                {
                    prescription.Status = PrescriptionStatus.Dispensed;
                }
                else if (dispensedCount > 0)
                {
                    prescription.Status = PrescriptionStatus.PartiallyDispensed;
                }
                else
                {
                    prescription.Status = PrescriptionStatus.Pending;
                }

                await _context.SaveChangesAsync();

                await LogUserAction(userId.Value, "DISPENSE_PRESCRIPTION", $"تم معالجة صرف الوصفة رقم {prescription.Id}");

                return Ok(new PrescriptionDispenseResponse
                {
                    Success = true,
                    Message = interactionResult.HasInteractions ? "تم تحديث حالة الصرف بنجاح (مع وجود تحذيرات تفاعل)" : "تم تحديث حالة الصرف بنجاح",
                    Warnings = interactionResult.Warnings,
                    Dispense = new PrescriptionDispenseInfo
                    {
                        Id = dispense.Id,
                        PrescriptionId = dispense.PrescriptionId,
                        PharmacistId = dispense.PharmacistId,
                        Notes = dispense.Notes,
                        DispenseDate = dispense.DispenseDate,
                        CreatedAt = dispense.CreatedAt,
                        PharmacistName = pharmacist.FullName,
                        Items = prescription.PrescriptionItems.Select(pi => new DispensedItemInfo
                        {
                            Id = pi.Id,
                            PrescriptionItemId = pi.Id,
                            MedicationName = pi.MedicationName,
                            Dosage = pi.Dosage,
                            Frequency = pi.Frequency,
                            Duration = pi.Duration,
                            Instructions = pi.Instructions,
                            Quantity = pi.Quantity,
                            IsDispensed = pi.IsDispensed
                        }).ToList()
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dispensing prescription");
                return StatusCode(500, new PrescriptionDispenseResponse { Success = false, Message = "حدث خطأ في الخادم" });
            }
        }

        /// <summary>
        /// إنشاء وصفة طبية جديدة (للصيادلة في حالات خاصة)
        /// </summary>
        [HttpPost("create-prescription")]
        public async Task<ActionResult<ServiceResult>> CreatePrescription([FromBody] PrescriptionCreateRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ServiceResult.Fail("البيانات المدخلة غير صحيحة"));

                // في العادة الطبيب هو من ينشئ الوصفة، ولكن إذا سمحنا للصيدلي، يجب التأكد من صحة البيانات
                var result = await _drugInteractionService.CreatePrescriptionAsync(request);

                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating prescription by pharmacist");
                return StatusCode(500, ServiceResult.Fail("حدث خطأ في الخادم"));
            }
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
                    return BadRequest(new PrescriptionStatusUpdateResponse { Success = false, Message = "البيانات غير صحيحة" });
                }

                var prescription = await _context.Prescriptions.FindAsync(request.PrescriptionId);
                if (prescription == null) return NotFound(new PrescriptionStatusUpdateResponse { Success = false, Message = "الوصفة غير موجودة" });

                prescription.Status = request.Status;
                await _context.SaveChangesAsync();

                return Ok(new PrescriptionStatusUpdateResponse
                {
                    Success = true,
                    Message = "تم تحديث الحالة بنجاح",
                    NewStatus = request.Status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating prescription status");
                return StatusCode(500, new PrescriptionStatusUpdateResponse { Success = false, Message = "حدث خطأ في الخادم" });
            }
        }

        /// <summary>
        /// تفعيل ملف المريض للصيادلة (والأطباء)
        /// </summary>
        [HttpPost("activate-patient-profile")]
        [Authorize(Roles = "Pharmacist,Doctor")]
        public async Task<ActionResult<LoginResponse>> ActivatePatientProfile()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var user = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == userId.Value);

            if (user == null) return NotFound("User not found.");

            if (user.Roles.Any(r => r.Role == UserRole.Patient))
            {
                return BadRequest(new { Success = false, Message = "لديك ملف طبي مفعل بالفعل." });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var patientProfile = new Patient
                {
                    UserId = user.Id,
                    FullName = user.FullName ?? "New Patient",
                    PatientCode = "PM-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(),
                    CreatedAt = DateTime.UtcNow
                };
                _context.Patients.Add(patientProfile);

                var roleAssignment = new UserRoleAssignment
                {
                    UserId = user.Id,
                    Role = UserRole.Patient
                };
                _context.UserRoleAssignments.Add(roleAssignment);
                user.Roles.Add(roleAssignment);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

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
                        Role = user.Role,
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
