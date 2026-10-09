using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using PetCareBooking.Application.Interfaces;

namespace PetCareBooking.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        public Guid? UserId
        {
            get
            {
                var idClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

                return Guid.TryParse(idClaim, out var id) ? id : null;
            }
        }

        public string? Email => User?.FindFirst(ClaimTypes.Email)?.Value
                             ?? User?.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

        public string? Role => User?.FindFirst(ClaimTypes.Role)?.Value;

        public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

        public bool IsInRole(string role) => User?.IsInRole(role) ?? false;

        public bool IsAdminOrStaff
        {
            get
            {
                var role = Role;
                if (string.IsNullOrEmpty(role)) return false;
                return role.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                       role.Equals("Staff", StringComparison.OrdinalIgnoreCase) ||
                       role.Equals("Manager", StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}

