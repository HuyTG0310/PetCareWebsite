using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Booking;
using PetCareBooking.Application.Features.Bookings.Commands.CancelBooking;
using PetCareBooking.Application.Features.Bookings.Commands.CheckInBooking;
using PetCareBooking.Application.Features.Bookings.Commands.CreateBooking;
using PetCareBooking.Application.Features.Bookings.Commands.UpdateBookingItemStatus;
using PetCareBooking.Application.Features.Bookings.Queries.GetAdminBookings;
using PetCareBooking.Application.Features.Bookings.Queries.GetBookingById;
using PetCareBooking.Application.Features.Bookings.Queries.GetCustomerBookings;
using PetCareBooking.Application.Features.Bookings.Queries.GetStaffGroomingTasks;
using PetCareBooking.Application.Features.Bookings.Queries.SearchBookings;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;

        public BookingsController(IMediator mediator, ICurrentUserService currentUserService)
        {
            _mediator = mediator;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// [Customer / Staff / Admin] Tạo đơn booking mới.
        /// Customer: tự động lấy CustomerId từ JWT token (không cần gửi customerId trong body).
        /// Staff/Admin: có thể truyền customerId trong body để đặt hộ khách tại quầy.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingCommand command)
        {
            if (!_currentUserService.IsAdminOrStaff && _currentUserService.UserId.HasValue)
            {
                command.CustomerId = _currentUserService.UserId.Value;
            }
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// [Customer ONLY] Lấy danh sách booking của chính mình (đọc từ JWT Token, an toàn 100%, không truyền Id qua URL)
        /// </summary>
        [HttpGet("my-bookings")]
        public async Task<IActionResult> GetMyBookings(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] BookingStatus? status = null)
        {
            if (!_currentUserService.UserId.HasValue)
            {
                return Unauthorized(new ApiResponse<PagedResult<BookingListResponseDTO>>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Invalid user token.",
                    Result = null
                });
            }

            var query = new GetCustomerBookingsQuery
            {
                CustomerId = _currentUserService.UserId.Value,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Status = status
            };
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// [Customer (Own Booking) / Staff / Admin] Xem chi tiết 1 đơn đặt theo ID (có kiểm tra quyền sở hữu chống IDOR)
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetBookingById(Guid id)
        {
            var response = await _mediator.Send(new GetBookingByIdQuery { Id = id });
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// [Staff/Admin or Own Customer] Lấy danh sách booking của một khách hàng theo CustomerId
        /// </summary>
        [HttpGet("customer/{customerId:guid}")]
        public async Task<IActionResult> GetCustomerBookings(
            Guid customerId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] BookingStatus? status = null)
        {
            var query = new GetCustomerBookingsQuery
            {
                CustomerId = customerId,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Status = status
            };
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// [Customer (Own Booking) / Staff / Admin] Hủy đơn đặt lịch (có kiểm tra quyền sở hữu chống IDOR)
        /// </summary>
        [HttpPut("{id:guid}/cancel")]
        public async Task<IActionResult> CancelBooking(Guid id, [FromBody] CancelBookingCommand command)
        {
            command.Id = id;
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// [Staff / Admin ONLY] Check-in toàn bộ các mục trong đơn (1-click check-in)
        /// </summary>
        [Authorize(Roles = "Admin,Staff,Manager")]
        [HttpPut("{id:guid}/check-in")]
        public async Task<IActionResult> CheckInBooking(Guid id)
        {
            var command = new CheckInBookingCommand { Id = id };
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// [Staff / Admin ONLY] Cập nhật trạng thái từng mục, gán nhân viên, phòng, ghi chú kết quả
        /// </summary>
        [Authorize(Roles = "Admin,Staff,Manager")]
        [HttpPut("{bookingId:guid}/items/{itemId:guid}/status")]
        public async Task<IActionResult> UpdateBookingItemStatus(
            Guid bookingId,
            Guid itemId,
            [FromBody] UpdateBookingItemStatusCommand command)
        {
            command.BookingId = bookingId;
            command.ItemId = itemId;
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// [Staff / Admin ONLY] Lấy danh sách ca làm việc Grooming được phân công cho nhân viên
        /// </summary>
        [Authorize(Roles = "Admin,Staff,Manager")]
        [HttpGet("staff/{staffId:guid}/grooming-tasks")]
        public async Task<IActionResult> GetStaffGroomingTasks(
            Guid staffId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] BookingItemStatus? status = null,
            [FromQuery] DateTime? date = null)
        {
            var query = new GetStaffGroomingTasksQuery
            {
                StaffId = staffId,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Status = status,
                Date = date
            };
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// [Staff / Admin ONLY] Lấy danh sách booking toàn hệ thống (phân trang, lọc thời gian, trạng thái)
        /// </summary>
        [Authorize(Roles = "Admin,Staff,Manager")]
        [HttpGet("admin")]
        public async Task<IActionResult> GetAdminBookings(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] BookingStatus? status = null,
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null)
        {
            var query = new GetAdminBookingsQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Status = status,
                DateFrom = dateFrom,
                DateTo = dateTo
            };
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// [Staff / Admin ONLY] Tìm kiếm booking toàn hệ thống theo từ khóa
        /// </summary>
        [Authorize(Roles = "Admin,Staff,Manager")]
        [HttpGet("search")]
        public async Task<IActionResult> SearchBookings(
            [FromQuery] string? keyword = null,
            [FromQuery] BookingStatus? status = null,
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new SearchBookingsQuery
            {
                Keyword = keyword,
                Status = status,
                DateFrom = dateFrom,
                DateTo = dateTo,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }
    }
}
