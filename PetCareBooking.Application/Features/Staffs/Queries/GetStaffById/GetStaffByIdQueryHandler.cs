using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Staff;
using PetCareBooking.Application.Interfaces;
using StaffEntity = PetCareBooking.Domain.Entities.Staff;

namespace PetCareBooking.Application.Features.Staffs.Queries.GetStaffById
{
    public class GetStaffByIdQueryHandler : IRequestHandler<GetStaffByIdQuery, ApiResponse<StaffDetailResponseDto>>
    {
        private readonly IGenericRepository<StaffEntity> _repository;

        public GetStaffByIdQueryHandler(IGenericRepository<StaffEntity> repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<StaffDetailResponseDto>> Handle(GetStaffByIdQuery request, CancellationToken cancellationToken)
        {
            var staff = await _repository.GetQueryable()
                .AsNoTracking()
                .Include(s => s.StaffRoles)
                    .ThenInclude(sr => sr.Role)
                .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

            if (staff == null)
            {
                return new ApiResponse<StaffDetailResponseDto>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Staff not found."
                };
            }

            var result = new StaffDetailResponseDto
            {
                Id = staff.Id,
                Email = staff.Email,
                FullName = staff.FullName,
                PhoneNumber = staff.PhoneNumber,
                Address = staff.Address,
                AvatarUrl = staff.AvatarUrl,
                YearsOfExperience = staff.YearsOfExperience,
                Status = staff.Status,
                RoleIds = staff.StaffRoles.Select(sr => sr.RoleId).ToList(),
                RoleNames = staff.StaffRoles
                    .Where(sr => sr.Role != null)
                    .Select(sr => sr.Role.RoleName)
                    .ToList()
            };

            return new ApiResponse<StaffDetailResponseDto>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Staff detail retrieved successfully.",
                Result = result
            };
        }
    }
}
