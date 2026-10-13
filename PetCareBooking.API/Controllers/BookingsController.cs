using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Booking;
using PetCareBooking.Application.Features.Bookings.Commands.CancelBooking;
using PetCareBooking.Application.Features.Bookings.Commands.CheckInBooking;
using PetCareBooking.Application.Features.Bookings.Commands.CheckoutBooking;
using PetCareBooking.Application.Features.Bookings.Commands.CreateBooking;
using PetCareBooking.Application.Features.Bookings.Commands.CreateVnPayPaymentUrl;
using PetCareBooking.Application.Features.Bookings.Commands.UpdateBookingItemStatus;
using PetCareBooking.Application.Features.Bookings.Queries.GetAdminBookings;
using PetCareBooking.Application.Features.Bookings.Queries.GetBookingById;
using PetCareBooking.Application.Features.Bookings.Queries.GetBookingPayments;
using PetCareBooking.Application.Features.Bookings.Queries.GetCheckoutSummary;
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
        [Authorize(Roles = "Admin,Receptionist,Boarding Staff,Grooming Staff")]
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
        [Authorize(Roles = "Admin,Receptionist,Boarding Staff,Grooming Staff")]
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
        /// [Grooming Staff ONLY] Lấy danh sách ca làm việc Grooming của chính mình (Tự động đọc StaffId từ JWT Token, chống IDOR tuyệt đối)
        /// </summary>
        [Authorize(Roles = "Admin,Grooming Staff,Receptionist")]
        [HttpGet("my-grooming-tasks")]
        public async Task<IActionResult> GetMyGroomingTasks(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] BookingItemStatus? status = null,
            [FromQuery] DateTime? date = null)
        {
            if (!_currentUserService.UserId.HasValue)
            {
                return Unauthorized(new ApiResponse<PagedResult<GroomingTaskResponseDTO>>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Invalid user token.",
                    Result = null
                });
            }

            var query = new GetStaffGroomingTasksQuery
            {
                StaffId = _currentUserService.UserId.Value,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Status = status,
                Date = date
            };
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// [Admin / Receptionist] Lấy danh sách ca làm việc Grooming của một nhân viên theo StaffId (có chống IDOR)
        /// </summary>
        [Authorize(Roles = "Admin,Receptionist,Grooming Staff")]
        [HttpGet("staff/{staffId:guid}/grooming-tasks")]
        public async Task<IActionResult> GetStaffGroomingTasks(
            Guid staffId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] BookingItemStatus? status = null,
            [FromQuery] DateTime? date = null)
        {
            // Chống IDOR: Kỹ thuật viên Grooming chỉ được xem task của chính mình
            bool isAdminOrReceptionist = _currentUserService.IsInRole("Admin") ||
                                         _currentUserService.IsInRole("Receptionist");

            if (!isAdminOrReceptionist)
            {
                if (!_currentUserService.UserId.HasValue || _currentUserService.UserId.Value != staffId)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse<PagedResult<GroomingTaskResponseDTO>>
                    {
                        IsSuccess = false,
                        StatusCode = StatusCodes.Status403Forbidden,
                        Message = "You do not have permission to view grooming tasks of another staff member.",
                        Result = null
                    });
                }
            }

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
        [Authorize(Roles = "Admin,Receptionist,Boarding Staff,Grooming Staff")]
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
        [Authorize(Roles = "Admin,Receptionist,Boarding Staff,Grooming Staff")]
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

        /// <summary>
        /// [Customer / Staff / Admin] Xem tóm tắt hóa đơn thanh toán trước khi check-out (Tổng tiền, giảm giá, số tiền đã cọc/trả, số tiền còn lại)
        /// </summary>
        [HttpGet("{id:guid}/checkout-summary")]
        public async Task<IActionResult> GetCheckoutSummary(Guid id)
        {
            var query = new GetCheckoutSummaryQuery(id);
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// [Staff / Admin ONLY] Thực hiện check-out & thanh toán (tiền mặt / chuyển khoản), tạo PaymentTransaction, chuyển trạng thái Completed và tự động giải phóng phòng
        /// </summary>
        [Authorize(Roles = "Admin,Receptionist")]
        [HttpPost("{id:guid}/checkout")]
        public async Task<IActionResult> CheckoutBooking(Guid id, [FromBody] CheckoutBookingCommand command)
        {
            command.BookingId = id;
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }


        /// <summary>
        /// [Customer / Staff / Admin] Xem lịch sử các giao dịch thanh toán & đặt cọc của đơn đặt lịch
        /// </summary>
        [HttpGet("{id:guid}/payments")]
        public async Task<IActionResult> GetBookingPayments(Guid id)
        {
            var query = new GetBookingPaymentsQuery(id);
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// [Customer / Staff / Admin] Sinh đường dẫn thanh toán VNPay trực tuyến (dùng khi khách muốn thanh toán online sau khi tạo đơn)
        /// </summary>
        [HttpPost("{id:guid}/create-vnpay-url")]
        public async Task<IActionResult> CreateVnPayUrl(
            Guid id,
            [FromBody(EmptyBodyBehavior = Microsoft.AspNetCore.Mvc.ModelBinding.EmptyBodyBehavior.Allow)] CreateVnPayPaymentUrlCommand? command = null)
        {
            command ??= new CreateVnPayPaymentUrlCommand();
            command.BookingId = id;
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }
    }
}
