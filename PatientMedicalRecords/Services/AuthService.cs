using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PatientMedicalRecords.Data;
using PatientMedicalRecords.DTOs;
using PatientMedicalRecords.Models;
using System.Text;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using ZXing.Aztec.Internal;
using Microsoft.AspNetCore.Identity;

namespace PatientMedicalRecords.Services
{
    // واجهة خدمة الاعتماد (يمكن تعديلها إذا لديك واجهة حالية)
    public interface IAuthService
    {
        Task<ServiceResult> RegisterPatientAsync(PatientRegisterRequest request);
        Task<ServiceResult> RegisterDoctorAsync(DoctorRegisterRequest request);
        Task<ServiceResult> RegisterPharmacistAsync(PharmacistRegisterRequest request);
        Task<LoginResult> LoginAsync(LoginRequest request);
        Task<RefreshResult> RefreshTokenAsync(string refreshToken);
        Task<RefreshResult> AccessTokenAsync(string accessToken);
        Task<ServiceResult> LogoutAsync(string refreshToken);
        Task<ServiceResult> ApproveUserAsync(int userId, int approverId);
        Task<ChangePasswordResponse> ChangePasswordAsync(int userId, ChangePasswordRequest request);
        Task<bool> ValidateTokenAsync(string token);
        Task<UserInfo?> GetUserInfoAsync(int userId);

    }

    // نتائج مساعدة
    public class ServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }

        public static ServiceResult Ok(string msg = "Ok", object? data = null) => new ServiceResult { Success = true, Message = msg, Data = data };
        public static ServiceResult Fail(string msg = "Failed") => new ServiceResult { Success = false, Message = msg };
    }

    public class LoginResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public User? User { get; set; }
        public string? RoleName { get; set; }

        public static LoginResult Fail(string msg = "Invalid credentials") => new LoginResult { Success = false, Message = msg };
        public static LoginResult Ok(string access, string refresh, User user, string role) =>
            new LoginResult { Success = true, Message = "Logged in", AccessToken = access, RefreshToken = refresh, User = user, RoleName = role };
    }

    public class RefreshResult
    {
        public bool Success { get; set; }
        public string AccessToken { get; set; }
        public string? NewRefreshToken { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    // AuthService implementation
    public class AuthService : IAuthService
    {
        private readonly MedicalRecordsDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly INotificationService _notificationService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;
        private readonly IWebHostEnvironment _env;

        // قارن بالـ DI في Program.cs لتسجيل الخدمات
        public AuthService(MedicalRecordsDbContext context, IJwtService jwtService,
            INotificationService notificationService, IConfiguration configuration, IWebHostEnvironment env,
            ILogger<AuthService> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _notificationService = notificationService;
            _configuration = configuration;
            _logger = logger;
            _env = env;
        }


        //******************************
        // دالة مساعدة لحفظ الملف وإرجاع معلوماته
        private async Task<(string FilePath, string FileName, long FileSize, string ContentType)> SaveAttachmentAsync(IFormFile file)
        {
            // إنشاء مسار آمن وفريد للملف
            var uploadsFolder = Path.Combine(_env.ContentRootPath, "Attachments");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // إرجاع المسار النسبي لتخزينه في قاعدة البيانات
            var relativePath = Path.Combine("Attachments", uniqueFileName);
            return (relativePath, file.FileName, file.Length, file.ContentType);
        }
        //******************************



        // #region Registration

        public async Task<ServiceResult> RegisterPatientAsync(PatientRegisterRequest request)
        {
            // basic validation (يمكن تعزيزها)
            if (string.IsNullOrWhiteSpace(request.NationalId) || string.IsNullOrWhiteSpace(request.Password))
                return ServiceResult.Fail("Invalid data");

            if (request.Password != request.ConfirmPassword)
                return ServiceResult.Fail("Passwords do not match");

            var existing = await _context.Users.FirstOrDefaultAsync(u => u.NationalId == request.NationalId);
            if (existing != null) return ServiceResult.Fail("National ID already used.");

            // create user
            var user = new User
            {
                NationalId = request.NationalId,
                Role = UserRole.Patient,
                Status = UserStatus.Approved // patient auto-approved per your decision (no OTP)
            };

            // Hash password using BCrypt (project already used BCrypt per inspection)
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // create patient record
            var patient = new Patient
            {
                UserId = user.Id,
                FullName = request.FullName,
                DateOfBirth = request.DateOfBirth,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email
            };

            // generate unique patient code
            patient.PatientCode = await GenerateUniquePatientCodeAsync();

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            // create notification to patient with code (in-app)
            await _notificationService.CreateNotification(user.Id, "تم إنشاء حسابك", $"كود المريض الخاص بك: {patient.PatientCode}", new { patientCode = patient.PatientCode });

            // return patient code in response (optional)
            return ServiceResult.Ok("Patient registered", new { PatientCode = patient.PatientCode, UserId = user.Id });
        }

        public async Task<ServiceResult> RegisterDoctorAsync(DoctorRegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.NationalId) || string.IsNullOrWhiteSpace(request.Password))
                return ServiceResult.Fail("Invalid data");

            if (request.Password != request.ConfirmPassword)
                return ServiceResult.Fail("Passwords do not match");

            //****************
            var attachmentInfo = await SaveAttachmentAsync(request.LicenseDocument);
            var existing = await _context.Users.FirstOrDefaultAsync(u => u.NationalId == request.NationalId);
            if (existing != null) return ServiceResult.Fail("National ID already used.");
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 2. إنشاء المستخدم
                var user = new User
                {
                    NationalId = request.NationalId,
                    Role = UserRole.Doctor,
                    Status = UserStatus.Pending // require admin approval
                };
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                _context.Users.Add(user);
                await _context.SaveChangesAsync(); // حفظ للحصول على user.Id

                //****************                   

            var doctor = new Doctor
            {
                UserId = user.Id,
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                LicenseNumber = request.LicenseNumber,
                //LicenseDocumentUrl = request.LicenseDocumentUrl,
                Hospital = request.Hospital,
                Specialization = request.Specialization
            };

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();


            // notify doctor (pending)
            await _notificationService.CreateNotification(user.Id, "تم استلام طلب التسجيل", "حسابك تحت المراجعة من قبل الإدارة.");

                var attachment = new UserAttachment
                {
                    UserId = user.Id,
                    AttachmentType = "LicenseCertificate", // نوع ثابت لشهادة الترخيص
                    FilePath = attachmentInfo.FilePath,
                    FileName = attachmentInfo.FileName,
                    FileSize = attachmentInfo.FileSize,
                    ContentType = attachmentInfo.ContentType
                };
                _context.UserAttachments.Add(attachment);

                await _context.SaveChangesAsync(); // حفظ كل التغييرات
                await transaction.CommitAsync(); // تأكيد العملية
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // يمكنك هنا تسجيل الخطأ (log the exception)
                return ServiceResult.Fail("An error occurred during registration.");
            }

            // ... (إرسال الإشعار)

            return ServiceResult.Ok("Doctor registration submitted (pending approval)");
        }
        

        public async Task<ServiceResult> RegisterPharmacistAsync(PharmacistRegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.NationalId) || string.IsNullOrWhiteSpace(request.Password))
                return ServiceResult.Fail("Invalid data");

            if (request.Password != request.ConfirmPassword)
                return ServiceResult.Fail("Passwords do not match");

            var existing = await _context.Users.FirstOrDefaultAsync(u => u.NationalId == request.NationalId);
            if (existing != null) return ServiceResult.Fail("National ID already used.");

            var attachmentInfo = await SaveAttachmentAsync(request.LicenseDocument);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 2. إنشاء المستخدم
                var user = new User
                {
                    NationalId = request.NationalId,
                    Role = UserRole.Pharmacist,
                    Status = UserStatus.Pending,// require admin approval
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();            

            //user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            //_context.Users.Add(user);
            //await _context.SaveChangesAsync();

            var pharmacist = new Pharmacist
            {
                UserId = user.Id,
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                LicenseNumber = request.LicenseNumber,
                //LicenseDocumentUrl = request.LicenseDocumentUrl,
                PharmacyName = request.PharmacyName
            };

            _context.Pharmacists.Add(pharmacist);
            await _context.SaveChangesAsync();

            // notify pharmacist (pending)
            await _notificationService.CreateNotification(user.Id, "تم استلام طلب التسجيل", "حسابك تحت المراجعة من قبل الإدارة.");
                //***********
                var attachment = new UserAttachment
                {
                    UserId = user.Id,
                    AttachmentType = "LicenseCertificate",
                    FilePath = attachmentInfo.FilePath,
                    FileName = attachmentInfo.FileName,
                    FileSize = attachmentInfo.FileSize,
                    ContentType = attachmentInfo.ContentType
                };
                _context.UserAttachments.Add(attachment);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ServiceResult.Fail("An error occurred during registration.");
            }

            // ... (إرسال الإشعار)

            return ServiceResult.Ok("Pharmacist registration submitted (pending approval)");
        }
                //***********
            

        //#endregion

        #region Login / Refresh / Logout

        public async Task<LoginResult> LoginAsync(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Identifier) || string.IsNullOrWhiteSpace(request.Password))
                return LoginResult.Fail("Invalid credentials"); // generic

            User? user = null;

            // 1) Try find by patient code
            var patient = await _context.Patients.Include(p => p.User)
                                 .FirstOrDefaultAsync(p => p.PatientCode == request.Identifier);
            if (patient != null)
            {
                user = patient.User;
            }

            // 2) If not found, try by national id
            if (user == null)
            {
                user = await _context.Users.FirstOrDefaultAsync(u => u.NationalId == request.Identifier);
            }

            if (user == null)
                return LoginResult.Fail("Invalid credentials");

            // check status
            if (user.Status != UserStatus.Approved)
                return LoginResult.Fail("Account is not active");

            // verify password
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                // TODO: increment failed attempts and lockout if necessary
                return LoginResult.Fail("Invalid credentials");
            }

            // generate access token
            var accessToken = _jwtService.GenerateAccessToken(user);

            // generate secure refresh token
            var refreshTokenString = GenerateSecureToken();
            var refresh = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshTokenString,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                CreatedAt = DateTime.UtcNow,
            };

            _context.RefreshTokens.Add(refresh);
            await _context.SaveChangesAsync();

            // store device token for push notifications (if provided)
            if (!string.IsNullOrWhiteSpace(request.DeviceToken))
            {
                var existingDevice = await _context.DeviceTokens.FirstOrDefaultAsync(d => d.Token == request.DeviceToken && d.UserId == user.Id);
                if (existingDevice == null)
                {
                    _context.DeviceTokens.Add(new DeviceToken
                    {
                        UserId = user.Id,
                        Token = request.DeviceToken,
                        Platform = request.DevicePlatform,
                        CreatedAt = DateTime.UtcNow,
                        LastSeenAt = DateTime.UtcNow
                    });
                }
                else
                {
                    existingDevice.LastSeenAt = DateTime.UtcNow;
                }
                await _context.SaveChangesAsync();
            }

            // notification on login (optional)
            await _notificationService.CreateNotification(user.Id, "تم تسجيل الدخول", "تم تسجيل الدخول إلى حسابك بنجاح.");

            return LoginResult.Ok(accessToken, refreshTokenString, user, user.Role.ToString());
        
        }

        public async Task<RefreshResult> RefreshTokenAsync(string refreshToken)
        {
            var stored = await _context.RefreshTokens.Include(r => r.Id).FirstOrDefaultAsync(r => r.Token == refreshToken);
            if (stored == null) return new RefreshResult { Success = false, Message = "Invalid token" };
            if (stored.Revoked || stored.ExpiresAt <= DateTime.UtcNow) return new RefreshResult { Success = false, Message = "Invalid token" };

            // Optionally rotate: revoke old token and issue new one
            stored.Revoked = true;

            var newRefresh = new RefreshToken
            {
                UserId = stored.Id,
                Token = GenerateSecureToken(),
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                CreatedAt = DateTime.UtcNow
            };
            _context.RefreshTokens.Add(newRefresh);
            await _context.SaveChangesAsync();

            // generate new access token
            var access = _jwtService.GenerateAccessToken();

            return new RefreshResult { Success = true, AccessToken = access, NewRefreshToken = newRefresh.Token };
        }

        public async Task<ServiceResult> LogoutAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken)) return ServiceResult.Fail("No token");

            var stored = await _context.RefreshTokens.FirstOrDefaultAsync(r => r.Token == refreshToken);
            if (stored == null) return ServiceResult.Ok("Logged out");

            stored.Revoked = true;
            await _context.SaveChangesAsync();

            return ServiceResult.Ok("Logged out");
        }

        #endregion

        #region Admin actions

        public async Task<ServiceResult> ApproveUserAsync(int userId, int approverId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return ServiceResult.Fail("User not found");

            user.Status = UserStatus.Approved;
            await _context.SaveChangesAsync();

            // notify user
            await _notificationService.CreateNotification(user.Id, "تم تفعيل الحساب", "تم تفعيل حسابك من قبل الإدارة.");

            // optionally log who approved (could create audit log table)
            return ServiceResult.Ok("User approved");
        }

        #endregion

        #region Helpers

        private async Task<string> GenerateUniquePatientCodeAsync()
        {
            string code;
            do
            {
                code = "PM-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
            } while (await _context.Patients.AnyAsync(p => p.PatientCode == code));
            return code;
        }

        private string GenerateSecureToken(int size = 64)
        {
            // generate cryptographically secure random token (hex)
            var bytes = new byte[size];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Convert.ToHexString(bytes); // .NET 5+ method; or use BitConverter if older
        }

        #endregion


        ///********************************************************************
        ///
        public async Task<ChangePasswordResponse> ChangePasswordAsync(int userId, ChangePasswordRequest request)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return new ChangePasswordResponse
                    {
                        Success = false,
                        Message = "المستخدم غير موجود"
                    };
                }

                // Verify current password
                if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                {
                    return new ChangePasswordResponse
                    {
                        Success = false,
                        Message = "كلمة المرور الحالية غير صحيحة"
                    };
                }

                // Update password
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();


                // Log the password change
                //await LogUserAction(userId, "CHANGE_PASSWORD", "تم تغيير كلمة المرور");

                return new ChangePasswordResponse
                {
                    Success = true,
                    Message = "تم تغيير كلمة المرور بنجاح"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", userId);
                return new ChangePasswordResponse
                {
                    Success = false,
                    Message = "حدث خطأ أثناء تغيير كلمة المرور"
                };
            }
        }


        //************************************************************************
        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(GetJwtSecret());

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = GetJwtIssuer(),
                    ValidateAudience = true,
                    ValidAudience = GetJwtAudience(),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<UserInfo?> GetUserInfoAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return null;

                return new UserInfo
                {
                    Id = user.Id,
                    NationalId = user.NationalId,
                    Role = user.Role,
                    Status = user.Status,
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    Email = user.Email,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user info for user {UserId}", userId);
                return null;
            }
        }


        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(GetJwtSecret());
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                            new Claim(ClaimTypes.Name, user.NationalId),
                            new Claim(ClaimTypes.Role, user.Role.ToString()),
                            new Claim("Status", user.Status.ToString())
                        }),
                Expires = DateTime.UtcNow.AddMinutes(GetTokenExpirationMinutes()),
                Issuer = GetJwtIssuer(),
                Audience = GetJwtAudience(),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private async Task CreateRoleSpecificProfile(User user)
        {
            switch (user.Role)
            {
                case UserRole.Patient:
                    _context.Patients.Add(new Patient
                    {
                        UserId = user.Id,
                        FullName = user.FullName ?? "",
                        DateOfBirth = DateTime.UtcNow.AddYears(-30), // Default age
                        Gender = Gender.Male, // Default gender
                        CreatedAt = DateTime.UtcNow
                    });
                    break;

                case UserRole.Doctor:
                    _context.Doctors.Add(new Doctor
                    {
                        UserId = user.Id,
                        FullName = user.FullName ?? "",
                        CreatedAt = DateTime.UtcNow
                    });
                    break;

                case UserRole.Pharmacist:
                    _context.Pharmacists.Add(new Pharmacist
                    {
                        UserId = user.Id,
                        FullName = user.FullName ?? "",
                        CreatedAt = DateTime.UtcNow
                    });
                    break;
            }

            await _context.SaveChangesAsync();
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

        private string GetJwtSecret()
        {
            return _configuration["Jwt:Secret"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
        }

        private string GetJwtIssuer()
        {
            return _configuration["Jwt:Issuer"] ?? "PatientMedicalRecords";
        }

        private string GetJwtAudience()
        {
            return _configuration["Jwt:Audience"] ?? "PatientMedicalRecords";
        }

        private int GetTokenExpirationMinutes()
        {
            return int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60");
        }

        public Task<RefreshResult> AccessTokenAsync(string accessToken)
        {
            throw new NotImplementedException();
        }
    }






















}
 





































































































//using Microsoft.EntityFrameworkCore;
//using Microsoft.IdentityModel.Tokens;
//using Org.BouncyCastle.Crypto.Generators;
//using PatientMedicalRecords.Data;
//using PatientMedicalRecords.DTOs;
//using PatientMedicalRecords.Models;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;

//namespace PatientMedicalRecords.Services
//{
//    public interface IAuthService
//    {
//        Task<LoginResponse> LoginAsync(LoginRequest request);
//        Task<RegisterResponse> RegisterAsync(RegisterRequest request);
//        Task<ChangePasswordResponse> ChangePasswordAsync(int userId, ChangePasswordRequest request);
//        Task<bool> ValidateTokenAsync(string token);
//        Task<UserInfo?> GetUserInfoAsync(int userId);
//    }

//    public class AuthService : IAuthService
//    {
//        private readonly MedicalRecordsDbContext _context;
//        private readonly IConfiguration _configuration;
//        private readonly ILogger<AuthService> _logger;

//        public AuthService(
//            MedicalRecordsDbContext context,
//            IConfiguration configuration,
//            ILogger<AuthService> logger)
//        {
//            _context = context;
//            _configuration = configuration;
//            _logger = logger;
//        }

//        public async Task<LoginResponse> LoginAsync(LoginRequest request)
//        {
//            try
//            {
//                // Find user by national ID and role
//                var user = await _context.Users
//                    .FirstOrDefaultAsync(u => u.NationalId == request.NationalId && u.Role == request.Role);

//                if (user == null)
//                {
//                    return new LoginResponse
//                    {
//                        Success = false,
//                        Message = "الرقم الوطني أو الدور غير صحيح"
//                    };
//                }

//                // Check if user is approved
//                if (user.Status != UserStatus.Approved)
//                {
//                    return new LoginResponse
//                    {
//                        Success = false,
//                        Message = user.Status switch
//                        {
//                            UserStatus.Pending => "حسابك في انتظار الموافقة",
//                            UserStatus.Rejected => "تم رفض حسابك",
//                            UserStatus.Suspended => "حسابك معلق",
//                            _ => "حالة الحساب غير صحيحة"
//                        }
//                    };
//                }

//                // Verify password
//                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
//                {
//                    return new LoginResponse
//                    {
//                        Success = false,
//                        Message = "كلمة المرور غير صحيحة"
//                    };
//                }

//                // Update last login
//                user.LastLoginAt = DateTime.UtcNow;
//                await _context.SaveChangesAsync();

//                // Generate JWT token
//                var token = GenerateJwtToken(user);

//                // Log the login
//                await LogUserAction(user.Id, "LOGIN", "تم تسجيل الدخول بنجاح");

//                return new LoginResponse
//                {
//                    Success = true,
//                    Message = "تم تسجيل الدخول بنجاح",
//                    Token = token,
//                    ExpiresAt = DateTime.UtcNow.AddMinutes(GetTokenExpirationMinutes()),
//                    User = new UserInfo
//                    {
//                        Id = user.Id,
//                        NationalId = user.NationalId,
//                        Role = user.Role,
//                        Status = user.Status,
//                        FullName = user.FullName,
//                        PhoneNumber = user.PhoneNumber,
//                        Email = user.Email,
//                        CreatedAt = user.CreatedAt,
//                        LastLoginAt = user.LastLoginAt
//                    }
//                };
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error during login for user {NationalId}", request.NationalId);
//                return new LoginResponse
//                {
//                    Success = false,
//                    Message = "حدث خطأ أثناء تسجيل الدخول"
//                };
//            }
//        }

//        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
//        {
//            try
//            {
//                // Check if user already exists
//                var existingUser = await _context.Users
//                    .FirstOrDefaultAsync(u => u.NationalId == request.NationalId);

//                if (existingUser != null)
//                {
//                    return new RegisterResponse
//                    {
//                        Success = false,
//                        Message = "هذا الرقم الوطني مسجل مسبقاً"
//                    };
//                }

//                // Create new user
//                var user = new User
//                {
//                    NationalId = request.NationalId,
//                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
//                    Role = request.Role,
//                    Status = request.Role == UserRole.Patient ? UserStatus.Approved : UserStatus.Pending,
//                    CreatedAt = DateTime.UtcNow
//                };

//                _context.Users.Add(user);
//                await _context.SaveChangesAsync();

//                // Create role-specific profile
//                await CreateRoleSpecificProfile(user);

//                // Log the registration
//                await LogUserAction(user.Id, "REGISTER", "تم إنشاء حساب جديد");

//                return new RegisterResponse
//                {
//                    Success = true,
//                    Message = request.Role == UserRole.Patient 
//                        ? "تم إنشاء الحساب بنجاح" 
//                        : "تم إرسال طلب التسجيل، في انتظار الموافقة",
//                    UserId = user.Id
//                };
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error during registration for user {NationalId}", request.NationalId);
//                return new RegisterResponse
//                {
//                    Success = false,
//                    Message = "حدث خطأ أثناء إنشاء الحساب"
//                };
//            }
//        }

//        
//        public async Task<bool> ValidateTokenAsync(string token)
//        {
//            try
//            {
//                var tokenHandler = new JwtSecurityTokenHandler();
//                var key = Encoding.ASCII.GetBytes(GetJwtSecret());

//                tokenHandler.ValidateToken(token, new TokenValidationParameters
//                {
//                    ValidateIssuerSigningKey = true,
//                    IssuerSigningKey = new SymmetricSecurityKey(key),
//                    ValidateIssuer = true,
//                    ValidIssuer = GetJwtIssuer(),
//                    ValidateAudience = true,
//                    ValidAudience = GetJwtAudience(),
//                    ValidateLifetime = true,
//                    ClockSkew = TimeSpan.Zero
//                }, out SecurityToken validatedToken);

//                return true;
//            }
//            catch
//            {
//                return false;
//            }
//        }

//        public async Task<UserInfo?> GetUserInfoAsync(int userId)
//        {
//            try
//            {
//                var user = await _context.Users.FindAsync(userId);
//                if (user == null) return null;

//                return new UserInfo
//                {
//                    Id = user.Id,
//                    NationalId = user.NationalId,
//                    Role = user.Role,
//                    Status = user.Status,
//                    FullName = user.FullName,
//                    PhoneNumber = user.PhoneNumber,
//                    Email = user.Email,
//                    CreatedAt = user.CreatedAt,
//                    LastLoginAt = user.LastLoginAt
//                };
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error getting user info for user {UserId}", userId);
//                return null;
//            }
//        }


//    // Additional DTOs for responses
//    public class ChangePasswordResponse
//    {
//        public bool Success { get; set; }
//        public string Message { get; set; } = string.Empty;
//    }
//}
