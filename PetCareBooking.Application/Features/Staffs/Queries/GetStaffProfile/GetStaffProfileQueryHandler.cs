using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Staff;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Enums;
using StaffEntity = PetCareBooking.Domain.Entities.Staff;

namespace PetCareBooking.Application.Features.Staffs.Queries.GetStaffProfile
{
    public class GetStaffProfileQueryHandler : IRequestHandler<GetStaffProfileQuery, ApiResponse<StaffProfileResponseDto>>
    {
        private readonly IGenericRepository<StaffEntity> _repository;
        private readonly ICurrentUserService _currentUserService;

        public GetStaffProfileQueryHandler(
            IGenericRepository<StaffEntity> repository,
            ICurrentUserService currentUserService)
        {
            _repository = repository;
            _currentUserService = currentUserService;
        }

        public async Task<ApiResponse<StaffProfileResponseDto>> Handle(GetStaffProfileQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (userId == null)
            {
                return new ApiResponse<StaffProfileResponseDto>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Unauthorized access."
                };
            }

            var staff = await _repository.GetQueryable()
                .AsNoTracking()
                .Include(s => s.StaffRoles)
                    .ThenInclude(sr => sr.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(s => s.Id == userId.Value, cancellationToken);

            if (staff == null || staff.Status != StaffStatus.Active)
            {
                return new ApiResponse<StaffProfileResponseDto>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Staff account is inactive or not found."
                };
            }

            var roles = staff.StaffRoles
                .Where(sr => sr.Role != null)
                .Select(sr => sr.Role.RoleName)
                .Distinct()
                .ToList();

            var permissions = staff.StaffRoles
                .Where(sr => sr.Role != null)
                .SelectMany(sr => sr.Role.RolePermissions)
                .Where(rp => rp.Permission != null)
                .Select(rp => rp.Permission.PermissionCode)
                .Distinct()
                .ToList();

            var result = new StaffProfileResponseDto
            {
                Id = staff.Id,
                Email = staff.Email,
                FullName = staff.FullName,
                PhoneNumber = staff.PhoneNumber,
                Address = staff.Address,
                AvatarUrl = staff.AvatarUrl,
                YearsOfExperience = staff.YearsOfExperience,
                Status = staff.Status,
                Roles = roles,
                Permissions = permissions,
                CreatedAt = staff.CreatedAt,
                UpdatedAt = staff.UpdatedAt
            };

            return new ApiResponse<StaffProfileResponseDto>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Staff profile retrieved successfully.",
                Result = result
            };
        }
    }
}
