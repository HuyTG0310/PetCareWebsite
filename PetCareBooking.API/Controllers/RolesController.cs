using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCareBooking.Application.Features.Roles.Commands.UpdateRolePermissions;
using PetCareBooking.Application.Features.Roles.Queries.GetRolePermissions;
using PetCareBooking.Application.Features.Roles.Queries.GetRoles;

namespace PetCareBooking.API.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RolesController(IMediator mediator)
        {
            _mediator = mediator;
        }


        //  View role list

        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            var response = await _mediator.Send(new GetRolesQuery());
            return StatusCode(response.StatusCode, response);
        }


        //View role permissions

        [HttpGet("{roleId:guid}/permissions")]
        public async Task<IActionResult> GetRolePermissions(Guid roleId)
        {
            var response = await _mediator.Send(new GetRolePermissionsQuery(roleId));
            return StatusCode(response.StatusCode, response);
        }


        //Edit role permissions

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