using Microsoft.AspNetCore.Identity.Data;
using Microsoft.IdentityModel.Tokens;
using PatientMedicalRecords.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace PatientMedicalRecords.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private string _secret;
        private string _issuer;
        private string _audience;
        private int _expiresMinutes;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
            _secret = _configuration["JwtSettings:Key"]; //?? throw new ArgumentNullException("Jwt:Key not configured");
            if (string.IsNullOrEmpty(_secret))
                throw new Exception("❌ JWT Secret Key is NULL or EMPTY. Check appsettings.json!");
            _issuer = _configuration["JwtSettings:Issuer"] ?? "patient-medical";
            _audience = _configuration["JwtSettings:Audience"] ?? "patient-medical-audience";
            _expiresMinutes =
                int.TryParse(_configuration["JwtSettings:AccessTokenExpirationMinutes"], out var m) ? m : 15;
        }

        public string GenerateAccessToken(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));



            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // 26-01-2026: Include all roles to support multi-role users
            var roles = new HashSet<string>();

            // 1. Add primary role (legacy field)
            roles.Add(user.Role.ToString());

            // 2. Add all roles from assignments table
            if (user.Roles != null)
            {
                foreach (var roleAssignment in user.Roles)
                {
                    roles.Add(roleAssignment.Role.ToString());
                }
            }

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }


            // Optional: add nationalId as claim if needed (careful with PII)
            if (!string.IsNullOrEmpty(user.NationalId))
            {
                claims.Add(new Claim("nationalId", user.NationalId));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret)); //?? throw new Exception("JWT Key is missing"));

            //var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Jwt:Key");
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_expiresMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateAccessToken()
        {
            throw new NotImplementedException();
        }


    }
}





//var claims = new List<Claim>
//{
//    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
//    new Claim("userId", user.Id.ToString()),
//    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
//    new Claim(ClaimTypes.Role, user.Role.ToString()),
//    //new Claim("role", user.Role.ToString()),
//};




//public string GenerateAccessToken()
//{
//    throw new NotImplementedException();
//}

//Optional: Add ValidateToken implementation if you need server-side validation helper.