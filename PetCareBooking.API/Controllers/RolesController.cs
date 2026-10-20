using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCareBooking.API.Attributes;
using PetCareBooking.Application.Features.Roles.Commands.UpdateRolePermissions;
using PetCareBooking.Application.Features.Roles.Queries.GetRolePermissions;
using PetCareBooking.Application.Features.Roles.Queries.GetRoles;

namespace PetCareBooking.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RolesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // View role list
        [RequirePermission("ROLE_VIEW")]
        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            var response = await _mediator.Send(new GetRolesQuery());
            return StatusCode(response.StatusCode, response);
        }

        // View role permissions
        [RequirePermission("ROLE_VIEW")]
        [HttpGet("{roleId:guid}/permissions")]
        public async Task<IActionResult> GetRolePermissions(Guid roleId)
        {
            var response = await _mediator.Send(new GetRolePermissionsQuery(roleId));
            return StatusCode(response.StatusCode, response);
        }

        // Edit role permissions
        [RequirePermission("ROLE_MANAGE")]
        [HttpPut("{roleId:guid}/permissions")]
        public async Task<IActionResult> UpdateRolePermissions(Guid roleId, [FromBody] UpdateRolePermissionsCommand command)
        {
            command ??= new UpdateRolePermissionsCommand();
            command.RoleId = roleId;
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }
    }
}
