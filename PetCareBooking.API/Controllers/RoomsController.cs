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


        [RequirePermission("ROOM_MANAGE")]
        [HttpPost]
        public async Task<IActionResult> CreateRoom([FromBody] CreateRoomCommand command)
        {
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        [RequirePermission("ROOM_VIEW")]
        [HttpGet]
        public async Task<IActionResult> GetRooms([FromQuery] GetRoomsQuery query)
        {
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        [RequirePermission("ROOM_CHECK_AVAILABILITY", "ROOM_VIEW")]
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableRooms([FromQuery] GetAvailableRoomsQuery query)
        {
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        [AllowAnonymous]
        [HttpGet("check-availability")]
        public async Task<IActionResult> CheckRoomAvailability([FromQuery] CheckRoomAvailabilityQuery query)
        {
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        [RequirePermission("ROOM_VIEW")]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetRoomById(Guid id)
        {
            var query = new GetRoomByIdQuery { Id = id };
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        [RequirePermission("ROOM_MANAGE")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateRoom(Guid id, [FromBody] UpdateRoomCommand command)
        {
            command.Id = id;
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        [RequirePermission("ROOM_MANAGE")]
        [HttpPatch("{id:guid}/status")]
        public async Task<IActionResult> UpdateRoomStatus(Guid id, [FromBody] UpdateRoomStatusCommand command)
        {
            command.Id = id;
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

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
