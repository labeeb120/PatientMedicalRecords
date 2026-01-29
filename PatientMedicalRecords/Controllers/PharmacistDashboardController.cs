using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatientMedicalRecords.Data;
using PatientMedicalRecords.DTOs;
using PatientMedicalRecords.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PatientMedicalRecords.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Pharmacist")] // Ensure only pharmacists can access
    public class PharmacistDashboardController : ControllerBase
    {
        private readonly MedicalRecordsDbContext _context;
        private readonly ILogger<PharmacistDashboardController> _logger;

        public PharmacistDashboardController(MedicalRecordsDbContext context, ILogger<PharmacistDashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("queue")]
        public async Task<ActionResult<List<PrescriptionQueueDto>>> GetQueue()
        {
            var queue = await _context.Prescriptions
                .AsNoTracking()
                .Where(p => p.Status == PrescriptionStatus.Pending || p.Status == PrescriptionStatus.InReview)
                .Include(p => p.Patient)
                .Include(p => p.Doctor)
                .OrderBy(p => p.PrescriptionDate) // Oldest first
                .Select(p => new PrescriptionQueueDto
                {
                    PrescriptionId = p.Id,
                    PatientName = p.Patient.FullName,
                    DoctorName = p.Doctor.FullName,
                    PrescriptionDate = p.PrescriptionDate,
                    ItemCount = p.PrescriptionItems.Count,
                    Status = p.Status
                })
                .ToListAsync();

            return Ok(queue);
        }

        [HttpPost("prescriptions/{id}/claim")]
        public async Task<IActionResult> ClaimPrescription(int id)
        {
            var pharmacistId = GetCurrentUserId();
            if (pharmacistId == null) return Unauthorized();

            var prescription = await _context.Prescriptions.FirstOrDefaultAsync(p => p.Id == id);

            if (prescription == null) return NotFound("Prescription not found.");

            if (prescription.Status != PrescriptionStatus.Pending)
            {
                return BadRequest($"Cannot claim prescription. Current status is '{prescription.Status}'.");
            }

            if (prescription.AssignedPharmacistId.HasValue)
            {
                return Conflict("This prescription has already been claimed by another pharmacist.");
            }

            prescription.Status = PrescriptionStatus.InReview;
            prescription.AssignedPharmacistId = pharmacistId.Value;

            await _context.SaveChangesAsync();

            return Ok("Prescription claimed successfully and is now under review.");
        }

        [HttpPost("prescriptions/{id}/release")]
        public async Task<IActionResult> ReleasePrescription(int id)
        {
            var pharmacistId = GetCurrentUserId();
            if (pharmacistId == null) return Unauthorized();

            var prescription = await _context.Prescriptions.FirstOrDefaultAsync(p => p.Id == id);

            if (prescription == null) return NotFound("Prescription not found.");

            if (prescription.AssignedPharmacistId != pharmacistId.Value)
            {
                return Forbid("You cannot release a prescription that is not assigned to you.");
            }

            prescription.Status = PrescriptionStatus.Pending; // Return to the queue
            prescription.AssignedPharmacistId = null;

            await _context.SaveChangesAsync();

            return Ok("Prescription released back to the queue.");
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdClaim?.Value, out int userId) ? userId : (int?)null;
        }
    }
}
