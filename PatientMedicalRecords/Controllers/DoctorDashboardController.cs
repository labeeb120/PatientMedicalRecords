using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatientMedicalRecords.Data;
using PatientMedicalRecords.DTOs;
using PatientMedicalRecords.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace PatientMedicalRecords.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Doctor")] // Ensure only doctors can access this
    public class DoctorDashboardController : ControllerBase
    {
        private readonly MedicalRecordsDbContext _context;
        private readonly ILogger<DoctorDashboardController> _logger;

        public DoctorDashboardController(MedicalRecordsDbContext context, ILogger<DoctorDashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("insights")]
        public async Task<ActionResult<DoctorInsightsDto>> GetInsights()
        {
            var doctorId = GetCurrentUserId();
            if (doctorId == null) return Unauthorized();

            var today = DateTime.UtcNow.Date;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            var diagnoses = _context.MedicalRecords.Where(r => r.DoctorId == doctorId);
            var prescriptions = _context.Prescriptions.Where(p => p.DoctorId == doctorId);

            var patientsToday = await diagnoses.Where(r => r.RecordDate.Date == today)
                .Select(r => r.PatientId)
                .Union(prescriptions.Where(p => p.PrescriptionDate.Date == today).Select(p => p.PatientId))
                .Distinct().CountAsync();

            var patientsThisWeek = await diagnoses.Where(r => r.RecordDate.Date >= startOfWeek)
                .Select(r => r.PatientId)
                .Union(prescriptions.Where(p => p.PrescriptionDate.Date >= startOfWeek).Select(p => p.PatientId))
                .Distinct().CountAsync();

            var patientsThisMonth = await diagnoses.Where(r => r.RecordDate.Date >= startOfMonth)
                .Select(r => r.PatientId)
                .Union(prescriptions.Where(p => p.PrescriptionDate.Date >= startOfMonth).Select(p => p.PatientId))
                .Distinct().CountAsync();

            return Ok(new DoctorInsightsDto
            {
                PatientsToday = patientsToday,
                PatientsThisWeek = patientsThisWeek,
                PatientsThisMonth = patientsThisMonth
            });
        }

        [HttpGet("patients")]
        public async Task<ActionResult<List<PatientCardDto>>> GetPatients([FromQuery] string filter = "recent_activity")
        {
            var doctorId = GetCurrentUserId();
            if (doctorId == null) return Unauthorized();

            switch (filter.ToLower())
            {
                case "recent_activity":
                    return await GetRecentActivityPatients(doctorId.Value);
                case "pinned":
                    return await GetPinnedPatients(doctorId.Value);
                case "new_patients":
                    return await GetNewPatients();
                default:
                    return BadRequest("Invalid filter specified.");
            }
        }

      
        
        
        [HttpPost("patients/{patientId}/pin")]
        public async Task<IActionResult> PinPatient(int patientId)
        {
            var doctorId = GetCurrentUserId();
            if (doctorId == null) return Unauthorized();

            var patientExists = await _context.Patients.AnyAsync(p => p.UserId == patientId);
            if (!patientExists) return NotFound("Patient not found.");

            var alreadyPinned = await _context.PinnedPatients
                .AnyAsync(pp => pp.DoctorId == doctorId.Value && pp.PatientId == patientId);

            if (alreadyPinned) return Ok("Patient is already pinned.");

            var pinnedPatient = new PinnedPatient
            {
                DoctorId = doctorId.Value,
                PatientId = patientId
            };

            _context.PinnedPatients.Add(pinnedPatient);
            await _context.SaveChangesAsync();

            return Ok("Patient pinned successfully.");
        }

        [HttpDelete("patients/{patientId}/unpin")]
        public async Task<IActionResult> UnpinPatient(int patientId)
        {
            var doctorId = GetCurrentUserId();
            if (doctorId == null) return Unauthorized();

            var pinnedPatient = await _context.PinnedPatients
                .FirstOrDefaultAsync(pp => pp.DoctorId == doctorId.Value && pp.PatientId == patientId);

            if (pinnedPatient == null) return NotFound("Patient is not pinned.");

            _context.PinnedPatients.Remove(pinnedPatient);
            await _context.SaveChangesAsync();

            return Ok("Patient unpinned successfully.");
        }


        // Helper methods for GetPatients


        // ... (داخل DoctorDashboardController)

        private async Task<ActionResult<List<PatientCardDto>>> GetRecentActivityPatients(int doctorId)
        {
            try
            {
                // Step 1: Fetch all recent interactions from the database into memory.
                // This is a simple query that EF Core can easily translate to SQL.
                var recentDiagnoses = _context.MedicalRecords
                    .Where(r => r.DoctorId == doctorId)
                    .Select(r => new { r.PatientId, InteractionDate = r.RecordDate, Type = "Diagnosis" });

                var recentPrescriptions = _context.Prescriptions
                    .Where(p => p.DoctorId == doctorId)
                    .Select(p => new { p.PatientId, InteractionDate = p.PrescriptionDate, Type = "Prescription" });

                var allInteractions = await recentDiagnoses.Union(recentPrescriptions).ToListAsync();

                // Step 2: Now that the data is in memory (as a List), perform the complex grouping and selection using LINQ to Objects.
                // This code runs on your server, not in the database.
                var latestInteractionsPerPatient = allInteractions
                    .GroupBy(i => i.PatientId)
                    .Select(group => group.OrderByDescending(i => i.InteractionDate).First()) // Get the latest interaction from each group
                    .OrderByDescending(i => i.InteractionDate) // Order the patients themselves by their latest interaction
                    .Take(10) // Limit to the top 10 most recent patients
                    .ToList();

                if (!latestInteractionsPerPatient.Any())
                {
                    return Ok(new List<PatientCardDto>()); // Return an empty list if there are no interactions
                }

                // Step 3: Fetch the names and codes for only the patients we need.
                // This is a very efficient query that translates to `WHERE PatientId IN (...)` in SQL.
                var patientIds = latestInteractionsPerPatient.Select(i => i.PatientId).ToList();
                var patientsInfo = await _context.Patients
                    .Where(p => patientIds.Contains(p.UserId))
                    .Select(p => new { p.UserId, p.FullName, p.PatientCode })
                    .ToDictionaryAsync(p => p.UserId); // ToDictionary is very fast for lookups

                // Step 4: Combine the results to build the final DTO.
                var result = latestInteractionsPerPatient.Select(interaction => new PatientCardDto
                {
                    PatientId = interaction.PatientId,
                    FullName = patientsInfo.GetValueOrDefault(interaction.PatientId)?.FullName ?? "Unknown Patient",
                    PatientCode = patientsInfo.GetValueOrDefault(interaction.PatientId)?.PatientCode,
                    LastInteractionDate = interaction.InteractionDate,
                    LastInteractionType = interaction.Type
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent activity for doctor {DoctorId}", doctorId);
                return StatusCode(500, "An internal server error occurred while fetching recent patients.");
            }
        }


        // ... (باقي الكود في الكنترولر)














        private async Task<List<PatientCardDto>> GetPinnedPatients(int doctorId)
        {
            return await _context.PinnedPatients
                .Where(pp => pp.DoctorId == doctorId)
                .Include(pp => pp.Patient)
                .OrderByDescending(pp => pp.CreatedAt)
                .Select(pp => new PatientCardDto
                {
                    PatientId = pp.PatientId,
                    FullName = pp.Patient.FullName,
                    PatientCode = pp.Patient.PatientCode,
                    LastInteractionDate = pp.CreatedAt,
                    LastInteractionType = "Pinned"
                })
                .ToListAsync();
        }

        private async Task<List<PatientCardDto>> GetNewPatients()
        {
            return await _context.Patients
                .OrderByDescending(p => p.CreatedAt)
                .Take(10)
                .Select(p => new PatientCardDto
                {
                    PatientId = p.UserId,
                    FullName = p.FullName,
                    PatientCode = p.PatientCode,
                    LastInteractionDate = p.CreatedAt,
                    LastInteractionType = "New User"
                })
                .ToListAsync();
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdClaim?.Value, out int userId) ? userId : (int?)null;
        }
    }
}











//private async Task<List<PatientCardDto>> GetRecentActivityPatients(int doctorId)
//{
//    var recentDiagnoses = _context.MedicalRecords
//        .Where(r => r.DoctorId == doctorId)
//        .Select(r => new { r.PatientId, InteractionDate = r.RecordDate, Type = "Diagnosis" });

//    var recentPrescriptions = _context.Prescriptions
//        .Where(p => p.DoctorId == doctorId)
//        .Select(p => new { p.PatientId, InteractionDate = p.PrescriptionDate, Type = "Prescription" });

//    var recentInteractions = await recentDiagnoses.Union(recentPrescriptions)
//        .GroupBy(i => i.PatientId)
//        .Select(g => new
//        {
//            PatientId = g.Key,
//            LastInteraction = g.OrderByDescending(i => i.InteractionDate).First()
//        })
//        .OrderByDescending(x => x.LastInteraction.InteractionDate)
//        .Take(10)
//        .Join(_context.Patients,
//              interaction => interaction.PatientId,
//              patient => patient.UserId,
//              (interaction, patient) => new PatientCardDto
//              {
//                  PatientId = patient.UserId,
//                  FullName = patient.FullName,
//                  PatientCode = patient.PatientCode,
//                  LastInteractionDate = interaction.LastInteraction.InteractionDate,
//                  LastInteractionType = interaction.LastInteraction.Type
//              })
//        .ToListAsync();

//    return recentInteractions;
//}