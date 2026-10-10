using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Role;

namespace PetCareBooking.Application.Features.Roles.Queries.GetRolePermissions
{
    public class GetRolePermissionsQuery : IRequest<ApiResponse<RolePermissionDetailDto>>
    {
        public Guid RoleId { get; set; }

        public GetRolePermissionsQuery(Guid roleId)
        {
            RoleId = roleId;
        }
    }
}