using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.CareRecord;
using PetCareBooking.Application.Features.CareRecords.Commands.CreateCareRecord;
using PetCareBooking.Application.Features.CareRecords.Commands.DeleteCareRecord;
using PetCareBooking.Application.Features.CareRecords.Commands.UpdateCareRecord;
using PetCareBooking.Application.Features.CareRecords.Queries.GetCareRecordById;
using PetCareBooking.Application.Features.CareRecords.Queries.GetCareRecordsByBookingItem;
using PetCareBooking.Application.Features.CareRecords.Queries.GetCareRecordsWithPagination;
using PetCareBooking.Application.Features.CareRecords.Queries.SearchCareRecords;

namespace PetCareBooking.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CareRecordsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CareRecordsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// [Staff / Admin] Lấy danh sách nhật ký chăm sóc phân trang toàn hệ thống
        /// </summary>
        [Authorize(Roles = "Admin,Receptionist,Boarding Staff")]
        [HttpGet]
        public async Task<ActionResult<PagedResult<CareRecordListResponseDTO>>> GetCareRecords([FromQuery] GetCareRecordsWithPaginationQuery query)
        {
            var response = await _mediator.Send(query);
            return Ok(response);
        }

        /// <summary>
        /// [Staff / Admin] Tìm kiếm và lọc nhật ký chăm sóc theo từ khóa, nhân viên, hoặc mục đặt lịch
        /// </summary>
        [Authorize(Roles = "Admin,Receptionist,Boarding Staff")]
        [HttpGet("search")]
        public async Task<ActionResult<PagedResult<CareRecordListResponseDTO>>> SearchCareRecords([FromQuery] SearchCareRecordsQuery query)
        {
            var response = await _mediator.Send(query);
            return Ok(response);
        }

        /// <summary>
        /// [Customer / Staff / Admin] Lấy toàn bộ timeline nhật ký chăm sóc của một dịch vụ lưu trú cụ thể (có kiểm tra chống IDOR)
        /// </summary>
        [Authorize(Roles = "Admin,Receptionist,Boarding Staff,Grooming Staff,Customer")]
        [HttpGet("booking-item/{bookingItemId:guid}")]
        public async Task<IActionResult> GetCareRecordsByBookingItem(Guid bookingItemId)
        {
            var response = await _mediator.Send(new GetCareRecordsByBookingItemQuery(bookingItemId));
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// [Customer / Staff / Admin] Xem chi tiết một bản ghi nhật ký chăm sóc theo ID
        /// </summary>
        [Authorize(Roles = "Admin,Receptionist,Boarding Staff,Grooming Staff,Customer")]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetCareRecordById(Guid id)
        {
            var response = await _mediator.Send(new GetCareRecordByIdQuery { Id = id });
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// [Boarding Staff / Receptionist / Admin] Tạo mới một nhật ký chăm sóc cho thú cưng đang lưu trú (tự động nhận diện StaffId chống IDOR)
        /// </summary>
        [Authorize(Roles = "Admin,Boarding Staff,Receptionist")]
        [HttpPost]
        public async Task<IActionResult> CreateCareRecord([FromBody] CreateCareRecordCommand command)
        {
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// [Boarding Staff / Receptionist / Admin] Cập nhật bản ghi nhật ký chăm sóc
        /// </summary>
        [Authorize(Roles = "Admin,Boarding Staff,Receptionist")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateCareRecord(Guid id, [FromBody] UpdateCareRecordCommand command)
        {
            command.Id = id;
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// [Admin / Receptionist] Xóa một bản ghi nhật ký chăm sóc
        /// </summary>
        [Authorize(Roles = "Admin,Receptionist")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteCareRecord(Guid id)
        {
            var response = await _mediator.Send(new DeleteCareRecordCommand { Id = id });
            return StatusCode(response.StatusCode, response);
        }
    }
}
