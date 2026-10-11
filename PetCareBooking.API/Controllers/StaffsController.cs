using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCareBooking.Application.Features.Staffs.Commands.ChangeStaffPassword;
using PetCareBooking.Application.Features.Staffs.Commands.CreateStaff;
using PetCareBooking.Application.Features.Staffs.Commands.DeleteStaff;
using PetCareBooking.Application.Features.Staffs.Commands.UpdateStaff;
using PetCareBooking.Application.Features.Staffs.Commands.UpdateStaffProfile;
using PetCareBooking.Application.Features.Staffs.Queries.GetStaffById;
using PetCareBooking.Application.Features.Staffs.Queries.GetStaffProfile;
using PetCareBooking.Application.Features.Staffs.Queries.GetStaffsWithPagination;

namespace PetCareBooking.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StaffsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StaffsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // ==========================================
        // SECTION 1: ADMIN - MANAGE STAFF (UC-13)
        // ==========================================

        /// <summary>
        /// UC-13.1 & UC-13.6: View & Search staff list with pagination (Admin only)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetStaffs([FromQuery] GetStaffsWithPaginationQuery query)
        {
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// UC-13.2: View staff detail (Admin only)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetStaffById(Guid id)
        {
            var response = await _mediator.Send(new GetStaffByIdQuery(id));
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// UC-13.3: Add new staff (Admin only)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateStaff([FromBody] CreateStaffCommand command)
        {
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// UC-13.4: Edit staff profile and roles (Admin only)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateStaff(Guid id, [FromBody] UpdateStaffCommand command)
        {
            command ??= new UpdateStaffCommand();
            command.Id = id;
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// UC-13.5: Deactivate staff account (Admin only)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteStaff(Guid id)
        {
            var response = await _mediator.Send(new DeleteStaffCommand(id));
            return StatusCode(response.StatusCode, response);
        }

        // ==========================================
        // SECTION 2: STAFF - MANAGE OWN PROFILE
        // ==========================================

        /// <summary>
        /// View profile details of the authenticated staff
        /// </summary>
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var response = await _mediator.Send(new GetStaffProfileQuery());
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Update personal profile information of the authenticated staff
        /// </summary>
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateStaffProfileCommand command)
        {
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Change password for the authenticated staff
        /// </summary>
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangeStaffPasswordCommand command)
        {
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }
    }
}