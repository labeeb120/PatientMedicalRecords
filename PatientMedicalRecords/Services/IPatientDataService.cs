 using PatientMedicalRecords.DTOs;
using System.Threading.Tasks;
//25-01-2026 start: added interface for patient data service.
namespace PatientMedicalRecords.Services
    {
        public interface IPatientDataService
        {
            Task<PatientMedicalInfo?> GetPatientMedicalInfoAsync(int patientId);
        }
    }

