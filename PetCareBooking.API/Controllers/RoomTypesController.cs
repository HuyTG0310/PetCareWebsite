using MediatR;
using Microsoft.AspNetCore.Mvc;
using PetCareBooking.Application.Features.RoomTypes.Commands.CreateRoomType;
using PetCareBooking.Application.Features.RoomTypes.Commands.DeleteRoomType;
using PetCareBooking.Application.Features.RoomTypes.Commands.UpdateRoomType;
using PetCareBooking.Application.Features.RoomTypes.Queries.GetRoomTypeAvailability;
using PetCareBooking.Application.Features.RoomTypes.Queries.GetRoomTypeById;
using PetCareBooking.Application.Features.RoomTypes.Queries.GetRoomTypes;

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
        /// Create a new room type
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateRoomType([FromBody] CreateRoomTypeCommand command)
        {
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Get list of all room types (with room count and search by keyword)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetRoomTypes([FromQuery] string? searchTerm)
        {
            var query = new GetRoomTypesQuery
            {
                SearchTerm = searchTerm
            };
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// Kiểm tra số lượng phòng còn trống theo từng loại phòng trong khoảng thời gian lưu trú
        [HttpGet("availability")]
        public async Task<IActionResult> GetRoomTypeAvailability([FromQuery] GetRoomTypeAvailabilityQuery query)
        {
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Xem chi tiết một loại phòng theo ID (kèm danh sách các phòng thuộc loại này)
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetRoomTypeById(Guid id)
        {
            var query = new GetRoomTypeByIdQuery { Id = id };
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Update room type details by ID
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateRoomType(Guid id, [FromBody] UpdateRoomTypeCommand command)
        {
            command.Id = id;
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Delete room type by ID (only when no rooms belong to it)
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteRoomType(Guid id)
        {
            var command = new DeleteRoomTypeCommand { Id = id };
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }
    }
}
