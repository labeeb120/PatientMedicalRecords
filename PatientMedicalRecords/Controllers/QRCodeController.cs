using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientMedicalRecords.DTOs;
using PatientMedicalRecords.Services;

namespace PatientMedicalRecords.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QRCodeController : ControllerBase
    {
        private readonly IQRCodeService _qrCodeService;
        private readonly ILogger<QRCodeController> _logger;

        public QRCodeController(IQRCodeService qrCodeService, ILogger<QRCodeController> logger)
        {
            _qrCodeService = qrCodeService;
            _logger = logger;
        }

        /// <summary>
        /// التحقق من صحة رمز الوصول
        /// </summary>
        [HttpPost("validate-access")]
        public async Task<ActionResult<AccessResponse>> ValidateAccess([FromBody] AccessRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new AccessResponse
                    {
                        Success = false,
                        Message = "البيانات المدخلة غير صحيحة"
                    });
                }

                var result = await _qrCodeService.ValidateAccessTokenAsync(request);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating access token");
                return StatusCode(500, new AccessResponse
                {
                    Success = false,
                    Message = "حدث خطأ في الخادم"
                });
            }
        }

        /// <summary>
        /// طلب الوصول للمريض
        /// </summary>
        [HttpPost("request-access")]
        [Authorize]
        public async Task<ActionResult<AccessResponse>> RequestAccess([FromBody] AccessRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new AccessResponse
                    {
                        Success = false,
                        Message = "البيانات المدخلة غير صحيحة"
                    });
                }

                var result = await _qrCodeService.RequestAccessAsync(request.Token, GetCurrentUserId()!.Value);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting access");
                return StatusCode(500, new AccessResponse
                {
                    Success = false,
                    Message = "حدث خطأ في الخادم"
                });
            }
        }

        /// <summary>
        /// الموافقة على طلب الوصول
        /// </summary>
        [HttpPost("approve-access")]
        [Authorize]
        public async Task<ActionResult<ApprovalResponse>> ApproveAccess([FromBody] ApprovalRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApprovalResponse
                    {
                        Success = false,
                        Message = "البيانات المدخلة غير صحيحة"
                    });
                }

                var result = await _qrCodeService.ApproveAccessAsync(request, GetCurrentUserId()!.Value);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving access");
                return StatusCode(500, new ApprovalResponse
                {
                    Success = false,
                    Message = "حدث خطأ في الخادم"
                });
            }
        }

        /// <summary>
        /// إلغاء رمز الوصول
        /// </summary>
        [HttpPost("revoke-token")]
        [Authorize]
        public async Task<ActionResult<bool>> RevokeToken([FromBody] RevokeTokenRequest request)
        {
            try
            {
                var result = await _qrCodeService.RevokeTokenAsync(request.Token);
                return Ok(new { Success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking token");
                return StatusCode(500, new { Success = false });
            }
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return null;
        }
    }

    // Additional DTO for revoke token
    public class RevokeTokenRequest
    {
        public string Token { get; set; } = string.Empty;
    }
}
