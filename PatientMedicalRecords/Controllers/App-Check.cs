using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PatientMedicalRecords.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class App_Check : ControllerBase
    {
        // GET: api/<App_Check>
        
        [HttpGet("ping")]
        public ActionResult<ApiResponse> Ping()
        {
            return Ok(new ApiResponse
            {
                Success = true,
                Message = "API Ready"
            });
        }

        public class ApiResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; }
        }

       
    }
}
