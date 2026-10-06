using MediatR;
using Microsoft.AspNetCore.Mvc;
using PetCareBooking.Application.Features.RoomTypes.Commands.CreateRoomType;

namespace PetCareBooking.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomTypesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RoomTypesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Tạo mới một loại phòng (Room Type)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateRoomType([FromBody] CreateRoomTypeCommand command)
        {
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }
    }
}
