using MediatR;
using Microsoft.AspNetCore.Mvc;
using PetCareBooking.Application.Features.Rooms.Commands.CreateRoom;

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
        /// Tạo mới một phòng vật lý (Room)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateRoom([FromBody] CreateRoomCommand command)
        {
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }
    }
}
