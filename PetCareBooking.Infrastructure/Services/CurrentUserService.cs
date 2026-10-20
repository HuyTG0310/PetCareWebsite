using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IServiceProvider _serviceProvider;
        private HashSet<string>? _cachedRealtimePermissions;

        public CurrentUserService(
            IHttpContextAccessor httpContextAccessor,
            IServiceProvider serviceProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _serviceProvider = serviceProvider;
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

        public bool IsInRole(string role)
        {
            if (User == null || !IsAuthenticated) return false;
            return User.IsInRole(role);
        }

        public bool IsAdminOrStaff
        {
            get
            {
                if (User == null || !IsAuthenticated) return false;
                return User.IsInRole("Admin") ||
                       User.IsInRole("Receptionist") ||
                       User.IsInRole("Boarding Staff") ||
                       User.IsInRole("Grooming Staff");
            }
        }

        public List<string> Permissions
        {
            get
            {
                if (User == null || !IsAuthenticated) return new List<string>();
                EnsureRealtimePermissionsLoaded();
                if (_cachedRealtimePermissions != null)
                {
                    return _cachedRealtimePermissions.ToList();
                }
                return User.FindAll("permission").Select(c => c.Value).Distinct().ToList();
            }
        }

        public bool HasPermission(string permissionCode)
        {
            if (User == null || !IsAuthenticated) return false;
            if (IsInRole("Admin")) return true; // Admin role always has all permissions

            EnsureRealtimePermissionsLoaded();
            if (_cachedRealtimePermissions != null)
            {
                return _cachedRealtimePermissions.Contains(permissionCode);
            }

            // Fallback to JWT claims
            return User.HasClaim("permission", permissionCode);
        }

        private void EnsureRealtimePermissionsLoaded()
        {
            if (_cachedRealtimePermissions != null) return;
            if (!UserId.HasValue || !IsAdminOrStaff) return;

            try
            {
                // Query real-time permissions directly from Database via Staff repository
                using var scope = _serviceProvider.CreateScope();
                var staffRepo = scope.ServiceProvider.GetService<IGenericRepository<Staff>>();
                if (staffRepo != null)
                {
                    var staffList = staffRepo.FindAsync(
                        s => s.Id == UserId.Value,
                        "StaffRoles.Role.RolePermissions.Permission"
                    ).GetAwaiter().GetResult();

                    var staff = staffList.FirstOrDefault();
                    if (staff != null)
                    {
                        var perms = staff.StaffRoles
                            .SelectMany(sr => sr.Role.RolePermissions)
                            .Select(rp => rp.Permission.PermissionCode)
                            .Distinct()
                            .ToHashSet();

                        _cachedRealtimePermissions = perms;
                        return;
                    }
                }
            }
            catch
            {
                // In case DB query fails, fallback to JWT claims
            }

            // Fallback to JWT token claims
            _cachedRealtimePermissions = User?.FindAll("permission")
                .Select(c => c.Value)
                .ToHashSet() ?? new HashSet<string>();
        }
    }
}
