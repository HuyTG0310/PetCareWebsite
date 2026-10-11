using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;
using RoleEntity = PetCareBooking.Domain.Entities.Role;
using StaffEntity = PetCareBooking.Domain.Entities.Staff;

namespace PetCareBooking.Application.Features.Staffs.Commands.UpdateStaff
{
    public class UpdateStaffCommandHandler : IRequestHandler<UpdateStaffCommand, ApiResponse<bool>>
    {
        private readonly IGenericRepository<StaffEntity> _staffRepository;
        private readonly IGenericRepository<RoleEntity> _roleRepository;
        private readonly IGenericRepository<StaffRole> _staffRoleRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateStaffCommandHandler(
            IGenericRepository<StaffEntity> staffRepository,
            IGenericRepository<RoleEntity> roleRepository,
            IGenericRepository<StaffRole> staffRoleRepository,
            IUnitOfWork unitOfWork)
        {
            _staffRepository = staffRepository;
            _roleRepository = roleRepository;
            _staffRoleRepository = staffRoleRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<bool>> Handle(UpdateStaffCommand request, CancellationToken cancellationToken)
        {
            var staff = await _staffRepository.GetByIdAsync(request.Id);
            if (staff == null)
            {
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Staff not found.",
                    Result = false
                };
            }

            var normalizedPhone = request.PhoneNumber.Trim();
            var isPhoneTaken = await _staffRepository.GetQueryable()
                .AnyAsync(s => s.PhoneNumber == normalizedPhone && s.Id != staff.Id, cancellationToken);

            if (isPhoneTaken)
            {
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Phone number is already used by another staff.",
                    Result = false
                };
            }

            // Diffing logic for StaffRoles
            var requestedRoleIds = request.RoleIds.Distinct().ToList();
            var validRequestedRoleIds = await _roleRepository.GetQueryable()
                .Where(r => requestedRoleIds.Contains(r.Id))
                .Select(r => r.Id)
                .ToListAsync(cancellationToken);

            if (validRequestedRoleIds.Count != requestedRoleIds.Count)
            {
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "One or more assigned roles do not exist.",
                    Result = false
                };
            }

            // Update basic fields
            staff.FullName = request.FullName.Trim();
            staff.PhoneNumber = normalizedPhone;
            staff.Address = request.Address?.Trim();
            staff.AvatarUrl = request.AvatarUrl?.Trim();
            staff.YearsOfExperience = request.YearsOfExperience;
            staff.Status = request.Status;
            staff.UpdatedAt = DateTime.UtcNow;

            _staffRepository.Update(staff);

            var currentStaffRoles = await _staffRoleRepository.GetQueryable()
                .Where(sr => sr.StaffId == staff.Id)
                .ToListAsync(cancellationToken);

            var currentRoleIds = currentStaffRoles.Select(sr => sr.RoleId).ToList();

            // Roles to remove
            var rolesToRemove = currentStaffRoles
                .Where(sr => !validRequestedRoleIds.Contains(sr.RoleId))
                .ToList();

            foreach (var sr in rolesToRemove)
            {
                _staffRoleRepository.Delete(sr);
            }

            // Roles to add
            var roleIdsToAdd = validRequestedRoleIds
                .Where(id => !currentRoleIds.Contains(id))
                .ToList();

            foreach (var roleId in roleIdsToAdd)
            {
                await _staffRoleRepository.AddAsync(new StaffRole
                {
                    StaffId = staff.Id,
                    RoleId = roleId
                });
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<bool>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Staff updated successfully.",
                Result = true
            };
        }
    }
}