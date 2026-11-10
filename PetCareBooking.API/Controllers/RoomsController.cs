using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCareBooking.API.Attributes;
using PetCareBooking.Application.Features.Rooms.Commands.CreateRoom;
using PetCareBooking.Application.Features.Rooms.Commands.DeleteRoom;
using PetCareBooking.Application.Features.Rooms.Commands.UpdateRoom;
using PetCareBooking.Application.Features.Rooms.Commands.UpdateRoomStatus;
using PetCareBooking.Application.Features.Rooms.Queries.CheckRoomAvailability;
using PetCareBooking.Application.Features.Rooms.Queries.GetAvailableRooms;
using PetCareBooking.Application.Features.Rooms.Queries.GetRoomById;
using PetCareBooking.Application.Features.Rooms.Queries.GetRooms;

namespace PetCareBooking.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RoomsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create a new physical room
        /// </summary>
        [RequirePermission("ROOM_MANAGE")]
        [HttpPost]
        public async Task<IActionResult> CreateRoom([FromBody] CreateRoomCommand command)
        {
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Get list of rooms (paginated, filter by room type, status, and room name search)
        /// </summary>
        [RequirePermission("ROOM_VIEW")]
        [HttpGet]
        public async Task<IActionResult> GetRooms([FromQuery] GetRoomsQuery query)
        {
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Get list of specific available rooms within a stay period (for receptionist room assignment)
        /// </summary>
        [RequirePermission("ROOM_CHECK_AVAILABILITY", "ROOM_VIEW")]
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableRooms([FromQuery] GetAvailableRoomsQuery query)
        {
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// [Customer / Frontend Datepicker] Check room availability status per day in month or across date range.
        /// Used to disable/grey out fully-booked dates on calendar datepicker when customer selects Boarding service.
        /// </summary>
        [AllowAnonymous]
        [HttpGet("check-availability")]
        public async Task<IActionResult> CheckRoomAvailability([FromQuery] CheckRoomAvailabilityQuery query)
        {
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// View room details by ID
        /// </summary>
        [RequirePermission("ROOM_VIEW")]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetRoomById(Guid id)
        {
            var query = new GetRoomByIdQuery { Id = id };
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Update room information (room name, room type) by ID
        /// </summary>
        [RequirePermission("ROOM_MANAGE")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateRoom(Guid id, [FromBody] UpdateRoomCommand command)
        {
            command.Id = id;
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Quick update room status (Available <-> Maintenance)
        /// </summary>
        [RequirePermission("ROOM_MANAGE")]
        [HttpPatch("{id:guid}/status")]
        public async Task<IActionResult> UpdateRoomStatus(Guid id, [FromBody] UpdateRoomStatusCommand command)
        {
            command.Id = id;
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Delete room by ID (can only delete when there is no booking history)
        /// </summary>
        [RequirePermission("ROOM_MANAGE")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteRoom(Guid id)
        {
            var command = new DeleteRoomCommand { Id = id };
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }
    }
}
