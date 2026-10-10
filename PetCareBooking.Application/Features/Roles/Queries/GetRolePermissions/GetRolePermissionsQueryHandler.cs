using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Role;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;
using RoleEntity = PetCareBooking.Domain.Entities.Role;

namespace PetCareBooking.Application.Features.Roles.Queries.GetRolePermissions
{
    public class GetRolePermissionsQueryHandler : IRequestHandler<GetRolePermissionsQuery, ApiResponse<RolePermissionDetailDto>>
    {
        private readonly IGenericRepository<RoleEntity> _roleRepository;
        private readonly IGenericRepository<Permission> _permissionRepository;

        public GetRolePermissionsQueryHandler(
            IGenericRepository<RoleEntity> roleRepository,
            IGenericRepository<Permission> permissionRepository)
        {
            _roleRepository = roleRepository;
            _permissionRepository = permissionRepository;
        }

        public async Task<ApiResponse<RolePermissionDetailDto>> Handle(GetRolePermissionsQuery request, CancellationToken cancellationToken)
        {
            var role = await _roleRepository.GetQueryable()
                .AsNoTracking()
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);

            if (role == null)
            {
                return new ApiResponse<RolePermissionDetailDto>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Role not found."
                };
            }

            // Sắp xếp danh sách quyền đã gán theo ModuleName và PermissionCode
            var assignedPermissions = role.RolePermissions
                .Select(rp => rp.Permission)
                .OrderBy(p => p.ModuleName)
                .ThenBy(p => p.PermissionCode)
                .Select(p => new PermissionDto
                {
                    Id = p.Id,
                    PermissionCode = p.PermissionCode,
                    PermissionName = p.PermissionName,
                    ModuleName = p.ModuleName
                })
                .ToList();

            // Sắp xếp toàn bộ danh sách quyền hiện có theo ModuleName và PermissionCode
            var allPermissions = await _permissionRepository.GetQueryable()
                .AsNoTracking()
                .OrderBy(p => p.ModuleName)
                .ThenBy(p => p.PermissionCode)
                .Select(p => new PermissionDto
                {
                    Id = p.Id,
                    PermissionCode = p.PermissionCode,
                    PermissionName = p.PermissionName,
                    ModuleName = p.ModuleName
                })
                .ToListAsync(cancellationToken);

            var result = new RolePermissionDetailDto
            {
                RoleId = role.Id,
                RoleName = role.RoleName,
                Description = role.Description,
                AssignedPermissions = assignedPermissions,
                AllAvailablePermissions = allPermissions
            };

            return new ApiResponse<RolePermissionDetailDto>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Role permissions retrieved successfully.",
                Result = result
            };
        }
    }
}