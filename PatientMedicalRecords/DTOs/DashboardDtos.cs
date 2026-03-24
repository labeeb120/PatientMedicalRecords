using PatientMedicalRecords.Models;
using System;
using System.Collections.Generic;

namespace PatientMedicalRecords.DTOs
{
    // DTOs for Doctor's Dashboard
    public class DoctorInsightsDto
    {
        public int PatientsToday { get; set; }
        public int PatientsThisWeek { get; set; }
        public int PatientsThisMonth { get; set; }
    }

    public class PatientCardDto
    {
        public int PatientId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? PatientCode { get; set; }
        public DateTime LastInteractionDate { get; set; }
        public string LastInteractionType { get; set; } = string.Empty; // e.g., "Diagnosis", "Prescription"
    }

    // DTOs for Pharmacist's Dashboard
    public class PrescriptionQueueDto
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
        public int ItemCount { get; set; }
        public List<PrescriptionItemInfo> Items { get; set; } = new List<PrescriptionItemInfo>();
    }
}
