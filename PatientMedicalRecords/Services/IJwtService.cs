using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PatientMedicalRecords.Models;


namespace PatientMedicalRecords.Services
{
    public interface IJwtService
    {
        /// <summary>
        /// Generate a short-lived access token (JWT) for the given user.
        /// </summary>
        string GenerateAccessToken(User user);
        string GenerateAccessToken();
        /// <summary>
        /// Optional: validate JWT and return claims principal (not required now).
        /// </summary>
        // ClaimsPrincipal? ValidateToken(string token);
    }


   
}









