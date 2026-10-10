using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Role;
using PetCareBooking.Application.Interfaces;
using RoleEntity = PetCareBooking.Domain.Entities.Role;

namespace PetCareBooking.Application.Features.Roles.Queries.GetRoles
{
    public class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, ApiResponse<List<RoleResponseDto>>>
    {
        private readonly IGenericRepository<RoleEntity> _roleRepository;

        public GetRolesQueryHandler(IGenericRepository<RoleEntity> roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<ApiResponse<List<RoleResponseDto>>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
        {
            var roles = await _roleRepository.GetQueryable()
                .AsNoTracking()
                .Select(r => new RoleResponseDto
                {
                    Id = r.Id,
                    RoleName = r.RoleName,
                    Description = r.Description,
                    TotalPermissions = r.RolePermissions.Count
                })
                .ToListAsync(cancellationToken);

            return new ApiResponse<List<RoleResponseDto>>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Role list retrieved successfully.",
                Result = roles
            };
        }
    }
}