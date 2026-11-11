using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PetCareBooking.API.Attributes;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Booking;
using PetCareBooking.Application.Features.Bookings.Commands.CancelBooking;
using PetCareBooking.Application.Features.Bookings.Commands.CheckInBooking;
using PetCareBooking.Application.Features.Bookings.Commands.CheckoutBooking;
using PetCareBooking.Application.Features.Bookings.Commands.CreateBooking;
using PetCareBooking.Application.Features.Bookings.Commands.CreateVnPayPaymentUrl;
using PetCareBooking.Application.Features.Bookings.Commands.MarkBookingNoShow;
using PetCareBooking.Application.Features.Bookings.Commands.UpdateBookingItemStatus;
using PetCareBooking.Application.Features.Bookings.Commands.UpdateBookingStatus;
using PetCareBooking.Application.Features.Bookings.Queries.GetAdminBookings;
using PetCareBooking.Application.Features.Bookings.Queries.GetBookingById;
using PetCareBooking.Application.Features.Bookings.Queries.GetBookingPayments;
using PetCareBooking.Application.Features.Bookings.Queries.GetBookingReceipt;
using PetCareBooking.Application.Features.Bookings.Queries.GetCheckoutSummary;
using PetCareBooking.Application.Features.Bookings.Queries.GetCustomerBookings;
using PetCareBooking.Application.Features.Bookings.Queries.GetStaffBoardingTasks;
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
        /// [Customer / Staff / Admin] Create a new booking.
        /// Customer: CustomerId is automatically retrieved from the JWT token (no need to send customerId in request body).
        /// Staff/Admin: can provide customerId in the body to book on behalf of walk-in customers.
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
        /// [Customer ONLY] Get the list of own bookings (retrieved from JWT Token, 100% secure, no Id passed via URL)
        /// </summary>
        [HttpGet("my-bookings")]
        public async Task<IActionResult> GetMyBookings(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] BookingStatus? status = null,
            [FromQuery] DateTime? serviceDate = null,
            [FromQuery] ServiceType? serviceType = null)
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
                Status = status,
                ServiceDate = serviceDate,
                ServiceType = serviceType
            };
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// [Customer (Own Booking) / Staff / Admin] View booking details by ID (with ownership check to prevent IDOR)
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetBookingById(Guid id)
        {
            var response = await _mediator.Send(new GetBookingByIdQuery { Id = id });
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// [Staff/Admin or Own Customer] Get bookings of a customer by CustomerId
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
        /// [Customer (Own Booking) / Staff / Admin] Cancel booking (with ownership check to prevent IDOR)
        /// </summary>
        [HttpPut("{id:guid}/cancel")]
        public async Task<IActionResult> CancelBooking(Guid id, [FromBody] CancelBookingCommand command)
        {
            command.Id = id;
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// [Staff / Admin ONLY] Mark customer as No-Show according to the appointment schedule.
        /// Mark all items as No-Show, cancel the booking, and automatically release room/staff assignments.
        /// </summary>
        [Authorize(Roles = "Admin,Receptionist")]
        [HttpPut("{id:guid}/no-show")]
        public async Task<IActionResult> MarkBookingNoShow(
            Guid id,
            [FromBody(EmptyBodyBehavior = Microsoft.AspNetCore.Mvc.ModelBinding.EmptyBodyBehavior.Allow)] MarkBookingNoShowCommand? command = null)
        {
            command ??= new MarkBookingNoShowCommand();
            command.Id = id;
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// [Staff / Admin ONLY] Check-in all items in the booking (1-click check-in)
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
        /// [Staff / Admin ONLY] Update status of individual items, assign staff, room, and record service results
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
        /// [Staff / Admin ONLY] Update booking status (Pending -> Confirmed / Completed / Cancelled)
        /// </summary>
        [Authorize(Roles = "Admin,Receptionist")]
        [HttpPut("{id:guid}/status")]
        public async Task<IActionResult> UpdateBookingStatus(Guid id, [FromBody] UpdateBookingStatusCommand command)
        {
            command.Id = id;
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// [Grooming Staff ONLY] Get list of own grooming shifts/tasks (automatically reads StaffId from JWT Token, strictly preventing IDOR)
        /// </summary>
        [RequirePermission("GROOMING_TASK_VIEW")]
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
        /// [Admin / Receptionist] Get grooming task list of a specific staff by StaffId (with IDOR prevention)
        /// </summary>
        [RequirePermission("GROOMING_TASK_VIEW")]
        [HttpGet("staff/{staffId:guid}/grooming-tasks")]
        public async Task<IActionResult> GetStaffGroomingTasks(
            Guid staffId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] BookingItemStatus? status = null,
            [FromQuery] DateTime? date = null)
        {
            // Prevent IDOR: Grooming technician can only view their own tasks
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
        /// [Boarding Staff ONLY] Get list of own boarding shifts/rooms under charge (automatically reads StaffId from JWT Token, strictly preventing IDOR)
        /// </summary>
        [RequirePermission("CARE_RECORD_VIEW", "ROOM_VIEW")]
        [HttpGet("my-boarding-tasks")]
        public async Task<IActionResult> GetMyBoardingTasks(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] BookingItemStatus? status = null,
            [FromQuery] DateTime? date = null)
        {
            if (!_currentUserService.UserId.HasValue)
            {
                return Unauthorized(new ApiResponse<PagedResult<BoardingTaskResponseDTO>>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Invalid user token.",
                    Result = null
                });
            }

            var query = new GetStaffBoardingTasksQuery
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
        /// [Boarding Staff / Receptionist / Admin] Get list of all pets currently boarding at the facility (Checked-In or In-Progress)
        /// </summary>
        [RequirePermission("CARE_RECORD_VIEW", "ROOM_VIEW")]
        [HttpGet("active-boarding-tasks")]
        public async Task<IActionResult> GetActiveBoardingTasks(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] DateTime? date = null)
        {
            var query = new GetStaffBoardingTasksQuery
            {
                OnlyActiveGuests = true,
                Date = date ?? DateTime.UtcNow,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// [Admin / Receptionist] Get boarding task list by designated StaffId (with IDOR prevention check)
        /// </summary>
        [RequirePermission("CARE_RECORD_VIEW", "ROOM_VIEW")]
        [HttpGet("staff/{staffId:guid}/boarding-tasks")]
        public async Task<IActionResult> GetStaffBoardingTasks(
            Guid staffId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] BookingItemStatus? status = null,
            [FromQuery] DateTime? date = null)
        {
            bool isAdminOrReceptionist = _currentUserService.IsInRole("Admin") ||
                                         _currentUserService.IsInRole("Receptionist");

            if (!isAdminOrReceptionist)
            {
                if (!_currentUserService.UserId.HasValue || _currentUserService.UserId.Value != staffId)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse<PagedResult<BoardingTaskResponseDTO>>
                    {
                        IsSuccess = false,
                        StatusCode = StatusCodes.Status403Forbidden,
                        Message = "You do not have permission to view boarding tasks of another staff member.",
                        Result = null
                    });
                }
            }

            var query = new GetStaffBoardingTasksQuery
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
        /// [Staff / Admin ONLY] Get system-wide bookings list (paginated, filter by date range, status)
        /// </summary>
        [RequirePermission("BOOKING_VIEW")]
        [HttpGet("admin")]
        public async Task<IActionResult> GetAdminBookings(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] BookingStatus? status = null,
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null,
            [FromQuery] DateTime? serviceDate = null)
        {
            var query = new GetAdminBookingsQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Status = status,
                DateFrom = dateFrom,
                DateTo = dateTo,
                ServiceDate = serviceDate
            };
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// [Staff / Admin ONLY] Search system-wide bookings by keyword and service appointment date
        /// </summary>
        [RequirePermission("BOOKING_VIEW")]
        [HttpGet("search")]
        public async Task<IActionResult> SearchBookings(
            [FromQuery] string? keyword = null,
            [FromQuery] BookingStatus? status = null,
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null,
            [FromQuery] DateTime? serviceDate = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new SearchBookingsQuery
            {
                Keyword = keyword,
                Status = status,
                DateFrom = dateFrom,
                DateTo = dateTo,
                ServiceDate = serviceDate,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// [Customer / Staff / Admin] View checkout payment summary before checking out (Total, discount, deposit/paid amount, remaining balance)
        /// </summary>
        [HttpGet("{id:guid}/checkout-summary")]
        public async Task<IActionResult> GetCheckoutSummary(Guid id)
        {
            var query = new GetCheckoutSummaryQuery(id);
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// [Customer (Own Booking) / Staff / Admin] Export checkout invoice/printable receipt (for thermal receipt printing or e-invoice viewing)
        /// </summary>
        [HttpGet("{id:guid}/receipt")]
        public async Task<IActionResult> GetBookingReceipt(Guid id)
        {
            var query = new GetBookingReceiptQuery(id);
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// [Staff / Admin ONLY] Perform check-out & payment (cash / bank transfer), create PaymentTransaction, update status to Completed, and automatically release room
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
        /// [Customer / Staff / Admin] View payment and deposit transaction history of a booking
        /// </summary>
        [HttpGet("{id:guid}/payments")]
        public async Task<IActionResult> GetBookingPayments(Guid id)
        {
            var query = new GetBookingPaymentsQuery(id);
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// [Customer / Staff / Admin] Generate online VNPay payment URL (used when customer wants to pay online after booking creation)
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
