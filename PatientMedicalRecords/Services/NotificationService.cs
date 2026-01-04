using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using PatientMedicalRecords.Data;
using PatientMedicalRecords.Models;

namespace PatientMedicalRecords.Services
{
    public class NotificationService : INotificationService
    {
        private readonly MedicalRecordsDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public NotificationService(MedicalRecordsDbContext context, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task CreateNotification(int userId, string title, string? body = null, object? data = null)
        {
            var notif = new Notification
            {
                UserId = userId,
                Title = title,
                Body = body,
                DataJson = data != null ? JsonSerializer.Serialize(data) : null,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notif);
            await _context.SaveChangesAsync();

            // Try to push to devices (non-blocking)
            _ = PushToDeviceAsync(userId, title, body, data);
        }

        public async Task PushToDeviceAsync(int userId, string title, string? body = null, object? data = null)
        {
            // read FCM server key from config (optional)
            var fcmServerKey = _configuration["Push:FCMServerKey"];
            if (string.IsNullOrWhiteSpace(fcmServerKey))
            {
                // No push provider configured — nothing to do
                return;
            }

            // get device tokens for user
            var tokens = await _context.DeviceTokens
                .Where(d => d.UserId == userId)
                .Select(d => d.Token)
                .ToListAsync();

            if (tokens == null || tokens.Count == 0) return;

            // build payload per FCM legacy HTTP API (simpler). For production consider FCM HTTP v1 with OAuth.
            var payload = new
            {
                registration_ids = tokens,
                notification = new
                {
                    title = title,
                    body = body
                },
                data = data
            };

            var json = JsonSerializer.Serialize(payload);
            var req = new HttpRequestMessage(HttpMethod.Post, "https://fcm.googleapis.com/fcm/send");
            req.Headers.TryAddWithoutValidation("Authorization", $"key={fcmServerKey}");
            req.Content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var resp = await _httpClient.SendAsync(req);
                // optional: inspect result, log failures, remove invalid tokens
                if (!resp.IsSuccessStatusCode)
                {
                    // for debugging: read body
                    var respBody = await resp.Content.ReadAsStringAsync();
                    // TODO: parse response, remove tokens that FCM marks as invalid
                }
            }
            catch (Exception ex)
            {
                // swallow exceptions (do not break main flow). Log if you have logging infra.
            }
        }
    }
}
