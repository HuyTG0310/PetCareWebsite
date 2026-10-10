using MediatR;
using PetCareBooking.Application.Common.Models;
using System.Text.Json.Serialization;

namespace PetCareBooking.Application.Features.Roles.Commands.UpdateRolePermissions
{
    public class UpdateRolePermissionsCommand : IRequest<ApiResponse<bool>>
    {
        [JsonIgnore]
        public Guid RoleId { get; set; }

        public List<Guid> PermissionIds { get; set; } = new();
    }
}