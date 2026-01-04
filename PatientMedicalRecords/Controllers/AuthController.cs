using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientMedicalRecords.Data;
using PatientMedicalRecords.DTOs;
using PatientMedicalRecords.Services;
using System.Security.Claims;

namespace PatientMedicalRecords.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;           
        }
        [HttpPost("register/patient")]
        public async Task<IActionResult> RegisterPatient([FromBody] PatientRegisterRequest request)
        {
            var result = await _authService.RegisterPatientAsync(request);
            if (!result.Success) return BadRequest(result);
            return Created(string.Empty, result);
        }


        [HttpPost("register/doctor")]
        public async Task<IActionResult> RegisterDoctor([FromForm] DoctorRegisterRequest request) // <-- تغيير هنا
        {
            var result = await _authService.RegisterDoctorAsync(request);
            if (!result.Success) return BadRequest(result);
            return Created(string.Empty, result);
        }

        [HttpPost("register/pharmacist")]
        public async Task<IActionResult> RegisterPharmacist([FromForm] PharmacistRegisterRequest request) // <-- تغيير هنا
        {
            var result = await _authService.RegisterPharmacistAsync(request);
            if (!result.Success) return BadRequest(result);
            return Created(string.Empty, result);
        }




      

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            if (!result.Success) return Unauthorized(new { success = false, message = "not not not not"});

            // set refresh token cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // ensure HTTPS in production
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(30)
            };
            Response.Cookies.Append("refreshToken", result.AccessToken!, cookieOptions);

            return Ok(new
            {
                success = true,
                accessToken = result.AccessToken,
                role = result.RoleName,
                userId = result.User.Id
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            if (!Request.Cookies.TryGetValue("refreshToken", out var existingRefresh))
                return Unauthorized(new { success = false });

            var res = await _authService.RefreshTokenAsync(existingRefresh);
            if (!res.Success) return Unauthorized(new { success = false, message = res.Message });

            // rotate refresh token cookie if provided
            if (!string.IsNullOrEmpty(res.NewRefreshToken))
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(30)
                };
                Response.Cookies.Append("refreshToken", res.NewRefreshToken, cookieOptions);
            }

            return Ok(new { success = true, accessToken = res.AccessToken });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            // read cookie
            Request.Cookies.TryGetValue("refreshToken", out var refreshToken);
            if (!string.IsNullOrEmpty(refreshToken))
            {
                await _authService.LogoutAsync(refreshToken);
                // delete cookie
                Response.Cookies.Delete("refreshToken");
            }

            return Ok(new { success = true, message = "Logged out" });
        }

      
        [HttpPost("change-password")]
        public async Task<ActionResult<ChangePasswordResponse>> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ChangePasswordResponse
                    {
                        Success = false,
                        Message = "البيانات المدخلة غير صحيحة"
                    });
                }

                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new ChangePasswordResponse
                    {
                        Success = false,
                              Message = "غير مصرح لك بهذا الإجراء"
                    });
                }

          var result = await _authService.ChangePasswordAsync(userId.Value, request);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                         return Ok(result);
            }
             catch (Exception ex)
            {
                _logger.LogError(ex, "Error in change password endpoint");
return StatusCode(500, new ChangePasswordResponse
{
    Success = false,
    Message = "حدث خطأ في الخادم"
});
            }
        }

        //    /// <summary>
        //    /// الحصول على معلومات المستخدم الحالي
        //    /// </summary>
        [HttpGet("me")]
        public async Task<ActionResult<UserInfo>> GetCurrentUser()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized();
                }

                var userInfo = await _authService.GetUserInfoAsync(userId.Value);
                if (userInfo == null)
                {
                    return NotFound();
                }

                return Ok(userInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in get current user endpoint");
                return StatusCode(500, "حدث خطأ في الخادم");
            }
        }

        //    /// <summary>
        //    /// التحقق من صحة الرمز المميز
        //    /// </summary>
        [HttpPost("validate-token")]
        public async Task<ActionResult<bool>> ValidateToken([FromBody] ValidateTokenRequest request)
        {
            try
            {
                var isValid = await _authService.ValidateTokenAsync(request.Token);
                return Ok(new { IsValid = isValid });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in validate token endpoint");
                return StatusCode(500, "حدث خطأ في الخادم");
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
    }

            //// Additional DTO for token validation
            public class ValidateTokenRequest
        {
            public string Token { get; set; } = string.Empty;
        }






    }







//[HttpPost("register/doctor")]
//public async Task<IActionResult> RegisterDoctor([FromBody] DoctorRegisterRequest request)
//{
//    var result = await _authService.RegisterDoctorAsync(request);
//    if (!result.Success) return BadRequest(result);
//    return Created(string.Empty, result);
//}

//[HttpPost("register/pharmacist")]
//public async Task<IActionResult> RegisterPharmacist([FromBody] PharmacistRegisterRequest request)
//{
//    var result = await _authService.RegisterPharmacistAsync(request);
//    if (!result.Success) return BadRequest(result);
//    return Created(string.Empty, result);
//}





















//    /// <summary>
//    /// تسجيل الدخول
//    /// </summary>
//    [HttpPost("login")]
//    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
//    {
//        try
//        {
//            if (!ModelState.IsValid)
//            {
//                return BadRequest(new LoginResponse
//                {
//                    Success = false,
//                    Message = "البيانات المدخلة غير صحيحة"
//                });
//            }

//            var result = await _authService.LoginAsync(request);

//            if (!result.Success)
//            {
//                return BadRequest(result);
//            }

//            return Ok(result);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error in login endpoint");
//            return StatusCode(500, new LoginResponse
//            {
//                Success = false,
//                Message = "حدث خطأ في الخادم"
//            });
//        }
//    }

//    /// <summary>
//    /// إنشاء حساب جديد
//    /// </summary>
//    [HttpPost("register")]
//    public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest request)
//    {
//        try
//        {
//            if (!ModelState.IsValid)
//            {
//                return BadRequest(new RegisterResponse
//                {
//                    Success = false,
//                    Message = "البيانات المدخلة غير صحيحة"
//                });
//            }

//            var result = await _authService.RegisterAsync(request);

//            if (!result.Success)
//            {
//                return BadRequest(result);
//            }

//            return Ok(result);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error in register endpoint");
//            return StatusCode(500, new RegisterResponse
//            {
//                Success = false,
//                Message = "حدث خطأ في الخادم"
//            });
//        }
//    }


// Admin approve endpoint (ensure only admin can call)
//[Authorize(Roles = "Admin")]
//[HttpPost("/api/admin/approve-user")]
//public async Task<IActionResult> ApproveUser([FromBody] ApproveUserRequest request)
//{
//    // if you want audit info, get current admin id from claims
//    int approverId = 0;
//    if (User.Identity?.IsAuthenticated == true)
//    {
//        var idClaim = User.FindFirst("userId") ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
//        if (idClaim != null && int.TryParse(idClaim.Value, out var parsed))
//            approverId = parsed;
//    }

//    var res = await _authService.ApproveUserAsync(request.RequestId, approverId);
//    if (!res.Success) return BadRequest(res);
//    return Ok(res);
//}



//    /// <summary>
//    /// تغيير كلمة المرور
//    /// </summary>





