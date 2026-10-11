using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;
using PetCareBooking.Domain.Enums;
using RoleEntity = PetCareBooking.Domain.Entities.Role;
using StaffEntity = PetCareBooking.Domain.Entities.Staff;

namespace PetCareBooking.Application.Features.Staffs.Commands.CreateStaff
{
    public class CreateStaffCommandHandler : IRequestHandler<CreateStaffCommand, ApiResponse<Guid>>
    {
        private readonly IGenericRepository<StaffEntity> _staffRepository;
        private readonly IGenericRepository<RoleEntity> _roleRepository;
        private readonly IGenericRepository<StaffRole> _staffRoleRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateStaffCommandHandler(
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

        public async Task<ApiResponse<Guid>> Handle(CreateStaffCommand request, CancellationToken cancellationToken)
        {
            var normalizedEmail = request.Email.Trim().ToLower();
            var normalizedPhone = request.PhoneNumber.Trim();

            var emailExists = await _staffRepository.GetQueryable()
                .AnyAsync(s => s.Email.ToLower() == normalizedEmail, cancellationToken);

            if (emailExists)
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Email already exists."
                };
            }

            var phoneExists = await _staffRepository.GetQueryable()
                .AnyAsync(s => s.PhoneNumber == normalizedPhone, cancellationToken);

            if (phoneExists)
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Phone number already exists."
                };
            }

            var requestedRoleIds = request.RoleIds.Distinct().ToList();
            var validRoleIds = await _roleRepository.GetQueryable()
                .Where(r => requestedRoleIds.Contains(r.Id))
                .Select(r => r.Id)
                .ToListAsync(cancellationToken);

            if (validRoleIds.Count != requestedRoleIds.Count)
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "One or more assigned roles do not exist."
                };
            }

            var staff = new StaffEntity
            {
                Email = request.Email.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FullName = request.FullName.Trim(),
                PhoneNumber = normalizedPhone,
                Address = request.Address?.Trim(),
                AvatarUrl = request.AvatarUrl?.Trim(),
                YearsOfExperience = request.YearsOfExperience,
                Status = StaffStatus.Active
            };

            await _staffRepository.AddAsync(staff);

            foreach (var roleId in validRoleIds)
            {
                await _staffRoleRepository.AddAsync(new StaffRole
                {
                    StaffId = staff.Id,
                    RoleId = roleId
                });
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<Guid>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status201Created,
                Message = "Staff created successfully.",
                Result = staff.Id
            };
        }
    }
}