using MediatR;
using Microsoft.AspNetCore.Mvc;
using PetCareBooking.Application.Features.Rooms.Commands.CreateRoom;
using PetCareBooking.Application.Features.Rooms.Commands.DeleteRoom;
using PetCareBooking.Application.Features.Rooms.Commands.UpdateRoom;
using PetCareBooking.Application.Features.Rooms.Commands.UpdateRoomStatus;
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
        /// Tạo mới một phòng vật lý (Room)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateRoom([FromBody] CreateRoomCommand command)
        {
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Lấy danh sách các phòng (phân trang, lọc theo loại phòng, trạng thái, tìm kiếm tên phòng)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetRooms([FromQuery] GetRoomsQuery query)
        {
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Xem thông tin chi tiết một phòng theo ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetRoomById(Guid id)
        {
            var query = new GetRoomByIdQuery { Id = id };
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Cập nhật thông tin phòng (tên phòng, loại phòng) theo ID
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateRoom(Guid id, [FromBody] UpdateRoomCommand command)
        {
            command.Id = id;
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Cập nhật nhanh trạng thái phòng (Available <-> Maintenance)
        /// </summary>
        [HttpPatch("{id:guid}/status")]
        public async Task<IActionResult> UpdateRoomStatus(Guid id, [FromBody] UpdateRoomStatusCommand command)
        {
            command.Id = id;
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Xóa phòng theo ID (chỉ xóa được khi chưa có lịch sử đặt phòng)
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteRoom(Guid id)
        {
            var command = new DeleteRoomCommand { Id = id };
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }
    }
}
