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
        /// Tạo mới một loại phòng (Room Type)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateRoomType([FromBody] CreateRoomTypeCommand command)
        {
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Lấy danh sách tất cả loại phòng (kèm số lượng phòng và tìm kiếm theo từ khóa)
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

        /// <summary>
        /// Kiểm tra số lượng phòng còn trống theo từng loại phòng trong khoảng thời gian lưu trú
        /// </summary>
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
        /// Cập nhật thông tin loại phòng theo ID
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateRoomType(Guid id, [FromBody] UpdateRoomTypeCommand command)
        {
            command.Id = id;
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Xóa loại phòng theo ID (chỉ xóa khi không còn phòng nào bên trong)
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
