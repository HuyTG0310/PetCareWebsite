using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Auth;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.Features.Auth.Commands.StaffLogin
{
    public class StaffLoginCommandHandler : IRequestHandler<StaffLoginCommand, ApiResponse<StaffLoginResponseDto>>
    {
        private readonly IGenericRepository<Staff> _repository;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public StaffLoginCommandHandler(
            IGenericRepository<Staff> repository,
            IJwtTokenGenerator jwtTokenGenerator)
        {
            _repository = repository;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<ApiResponse<StaffLoginResponseDto>> Handle(StaffLoginCommand request, CancellationToken cancellationToken)
        {
            var normalizedEmail = request.Email.Trim().ToLower();

            // 1. Query Staff kèm theo quan hệ Role và Permission
            var staff = await _repository.GetQueryable()
                .Include(s => s.StaffRoles)
                    .ThenInclude(sr => sr.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(s => s.Email.ToLower() == normalizedEmail, cancellationToken);

            if (staff == null)
            {
                return new ApiResponse<StaffLoginResponseDto>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Invalid email or password."
                };
            }

            // 2. Kiểm tra mật khẩu (BCrypt)
            var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, staff.PasswordHash);
            if (!isPasswordValid)
            {
                return new ApiResponse<StaffLoginResponseDto>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Invalid email or password."
                };
            }

            // 3. Kiểm tra trạng thái hoạt động của nhân sự
            if (staff.Status != StaffStatus.Active)
            {
                return new ApiResponse<StaffLoginResponseDto>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Your account has been deactivated. Please contact Admin."
                };
            }

            // 4. Lấy danh sách Roles và Permissions không trùng lặp
            var roles = staff.StaffRoles
                .Select(sr => sr.Role.RoleName)
                .Distinct()
                .ToList();

            var permissions = staff.StaffRoles
                .SelectMany(sr => sr.Role.RolePermissions)
                .Select(rp => rp.Permission.PermissionCode)
                .Distinct()
                .ToList();

            // 5. Sinh JWT Token chứa đầy đủ Role và Permission
            var token = _jwtTokenGenerator.GenerateStaffToken(staff, roles, permissions);

            return new ApiResponse<StaffLoginResponseDto>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Staff login successful!",
                Result = new StaffLoginResponseDto
                {
                    Token = token,
                    StaffId = staff.Id,
                    FullName = staff.FullName,
                    Email = staff.Email,
                    Roles = roles,
                    Permissions = permissions
                }
            };
        }
    }
}