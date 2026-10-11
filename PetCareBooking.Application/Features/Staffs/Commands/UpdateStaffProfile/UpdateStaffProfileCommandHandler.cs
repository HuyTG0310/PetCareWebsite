using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Staff;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Enums;
using StaffEntity = PetCareBooking.Domain.Entities.Staff;

namespace PetCareBooking.Application.Features.Staffs.Commands.UpdateStaffProfile
{
    public class UpdateStaffProfileCommandHandler : IRequestHandler<UpdateStaffProfileCommand, ApiResponse<StaffProfileResponseDto>>
    {
        private readonly IGenericRepository<StaffEntity> _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public UpdateStaffProfileCommandHandler(
            IGenericRepository<StaffEntity> repository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<ApiResponse<StaffProfileResponseDto>> Handle(UpdateStaffProfileCommand request, CancellationToken cancellationToken)
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

            var normalizedPhone = request.PhoneNumber.Trim();
            var isPhoneTaken = await _repository.GetQueryable()
                .AnyAsync(s => s.PhoneNumber == normalizedPhone && s.Id != staff.Id, cancellationToken);

            if (isPhoneTaken)
            {
                return new ApiResponse<StaffProfileResponseDto>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Phone number is already associated with another staff account."
                };
            }

            staff.FullName = request.FullName.Trim();
            staff.PhoneNumber = normalizedPhone;
            staff.Address = request.Address?.Trim();
            staff.AvatarUrl = request.AvatarUrl?.Trim();
            staff.YearsOfExperience = request.YearsOfExperience;
            staff.UpdatedAt = DateTime.UtcNow;

            _repository.Update(staff);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

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

            return new ApiResponse<StaffProfileResponseDto>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Staff profile updated successfully.",
                Result = new StaffProfileResponseDto
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
                }
            };
        }
    }
}
