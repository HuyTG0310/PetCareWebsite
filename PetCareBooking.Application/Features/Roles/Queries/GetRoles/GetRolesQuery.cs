using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Role;

namespace PetCareBooking.Application.Features.Roles.Queries.GetRoles
{
    public class GetRolesQuery : IRequest<ApiResponse<List<RoleResponseDto>>>
    {
    }
}