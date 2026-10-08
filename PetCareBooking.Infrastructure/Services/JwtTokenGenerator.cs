using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PetCareBooking.Infrastructure.Services
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly IConfiguration _configuration;

        public JwtTokenGenerator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(Customer customer)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, customer.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, customer.Email),
                new Claim(ClaimTypes.Name, customer.FullName),
                new Claim(ClaimTypes.Role, "Customer")
            };

            return BuildToken(claims);
        }

        public string GenerateStaffToken(Staff staff, IEnumerable<string> roles, IEnumerable<string> permissions)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, staff.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, staff.Email),
                new Claim(ClaimTypes.Name, staff.FullName)
            };


            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }


            foreach (var permission in permissions)
            {
                claims.Add(new Claim("permission", permission));
            }

            return BuildToken(claims);
        }

        private string BuildToken(IEnumerable<Claim> claims)
        {
            var secretKey = _configuration["JwtSettings:SecretKey"] ?? "PetCareBooking_Secret_Key_Super_Secure_Key_2026_SWD392";
            var issuer = _configuration["JwtSettings:Issuer"] ?? "PetCareBookingAPI";
            var audience = _configuration["JwtSettings:Audience"] ?? "PetCareBookingClient";
            var expiryMinutes = double.Parse(_configuration["JwtSettings:ExpiryMinutes"] ?? "120");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}