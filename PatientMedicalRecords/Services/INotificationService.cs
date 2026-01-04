namespace PatientMedicalRecords.Services
{
    public interface INotificationService
    {
        Task CreateNotification(int userId, string title, string? body = null, object? data = null);
        Task PushToDeviceAsync(int userId, string title, string? body = null, object? data = null);
    }
}
