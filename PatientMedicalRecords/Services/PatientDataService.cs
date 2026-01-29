using Microsoft.EntityFrameworkCore;
using PatientMedicalRecords.Data;
using PatientMedicalRecords.DTOs;
using PatientMedicalRecords.Models;
using System.Linq;
using System.Threading.Tasks;
//25-01-2026 start: added implementation for patient data service.
namespace PatientMedicalRecords.Services
{
    public class PatientDataService : IPatientDataService
    {
        private readonly MedicalRecordsDbContext _context;

        public PatientDataService(MedicalRecordsDbContext context)
        {
            _context = context;
        }

        public async Task<PatientMedicalInfo?> GetPatientMedicalInfoAsync(int patientId)
        {
            var patient = await _context.Patients
                .AsNoTracking() // Use AsNoTracking for read-only queries to improve performance
                .Include(p => p.User)
                .Include(p => p.Allergies)
                .Include(p => p.ChronicDiseases)
                .Include(p => p.Surgeries)
                .Include(p => p.MedicalRecords).ThenInclude(mr => mr.Doctor)
                .Include(p => p.Prescriptions).ThenInclude(pr => pr.Doctor)
                .Include(p => p.Prescriptions).ThenInclude(pr => pr.PrescriptionItems)
                .FirstOrDefaultAsync(p => p.UserId == patientId);

            if (patient == null)
            {
                return null;
            }

            // This mapping logic can be simplified using AutoMapper in a real project
            return new PatientMedicalInfo
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
                PatientCode = patient.PatientCode,
                Allergies = patient.Allergies.Select(a => new AllergyInfo { /* Map properties */ }).ToList(),
                ChronicDiseases = patient.ChronicDiseases.Select(cd => new ChronicDiseaseInfo { /* Map properties */ }).ToList(),
                Surgeries = patient.Surgeries.Select(s => new SurgeryInfo { /* Map properties */ }).ToList(),
                MedicalRecords = patient.MedicalRecords.Select(mr => new MedicalRecordInfo
                {
                    Id = mr.Id,
                    Diagnosis = mr.Diagnosis,
                    Notes = mr.Notes,
                    RecordDate = mr.RecordDate,
                    DoctorName = mr.Doctor?.FullName ?? "N/A"
                }).ToList(),
                Prescriptions = patient.Prescriptions.Select(p => new PrescriptionInfo
                {
                    Id = p.Id,
                    Diagnosis = p.Diagnosis,
                    Status = p.Status,
                    PrescriptionDate = p.PrescriptionDate,
                    DoctorName = p.Doctor?.FullName ?? "N/A",
                    Items = p.PrescriptionItems.Select(pi => new PrescriptionItemInfo { /* Map properties */ }).ToList()
                }).ToList()
            };
        }
    }
}
