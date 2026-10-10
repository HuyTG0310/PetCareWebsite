using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;
using RoleEntity = PetCareBooking.Domain.Entities.Role;

namespace PetCareBooking.Application.Features.Roles.Commands.UpdateRolePermissions
{
    public class UpdateRolePermissionsHandler : IRequestHandler<UpdateRolePermissionsCommand, ApiResponse<bool>>
    {
        private readonly IGenericRepository<RoleEntity> _roleRepository;
        private readonly IGenericRepository<RolePermission> _rolePermissionRepository;
        private readonly IGenericRepository<Permission> _permissionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateRolePermissionsHandler(
            IGenericRepository<RoleEntity> roleRepository,
            IGenericRepository<RolePermission> rolePermissionRepository,
            IGenericRepository<Permission> permissionRepository,
            IUnitOfWork unitOfWork)
        {
            _roleRepository = roleRepository;
            _rolePermissionRepository = rolePermissionRepository;
            _permissionRepository = permissionRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<bool>> Handle(UpdateRolePermissionsCommand request, CancellationToken cancellationToken)
        {
            // 1. Check if Role exists
            var role = await _roleRepository.GetByIdAsync(request.RoleId);
            if (role == null)
            {
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Role not found."
                };
            }

            // 2. Filter valid Permission IDs from Database
            var validRequestedPermissionIds = await _permissionRepository.GetQueryable()
                .Where(p => request.PermissionIds.Contains(p.Id))
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);

            // 3. Prevent removing all permissions from the Admin role
            if (role.RoleName.Equals("Admin", StringComparison.OrdinalIgnoreCase) && !validRequestedPermissionIds.Any())
            {
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Cannot remove all permissions from the Admin role."
                };
            }

            // 4. Get currently assigned RolePermissions
            var currentRolePermissions = await _rolePermissionRepository.GetQueryable()
                .Where(rp => rp.RoleId == request.RoleId)
                .ToListAsync(cancellationToken);

            var currentPermissionIds = currentRolePermissions.Select(rp => rp.PermissionId).ToList();

            // 5. Apply Diffing Algorithm
            // 5a. Identify permissions to REMOVE
            var permissionsToRemove = currentRolePermissions
                .Where(rp => !validRequestedPermissionIds.Contains(rp.PermissionId))
                .ToList();

            foreach (var rp in permissionsToRemove)
            {
                _rolePermissionRepository.Delete(rp);
            }

            // 5b. Identify permissions to ADD
            var permissionIdsToAdd = validRequestedPermissionIds
                .Where(id => !currentPermissionIds.Contains(id))
                .ToList();

            foreach (var permissionId in permissionIdsToAdd)
            {
                await _rolePermissionRepository.AddAsync(new RolePermission
                {
                    RoleId = request.RoleId,
                    PermissionId = permissionId
                });
            }

            // 6. Save changes to DB
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<bool>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Role permissions updated successfully.",
                Result = true
            };
        }
    }
}