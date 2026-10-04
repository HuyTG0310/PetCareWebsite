using MediatR;
using Microsoft.AspNetCore.Mvc;
using PetCareBooking.Application.Features.Bookings.Commands.CancelBooking;
using PetCareBooking.Application.Features.Bookings.Commands.CreateBooking;
using PetCareBooking.Application.Features.Bookings.Commands.UpdateBookingItemStatus;
using PetCareBooking.Application.Features.Bookings.Queries.GetAdminBookings;
using PetCareBooking.Application.Features.Bookings.Queries.GetBookingById;
using PetCareBooking.Application.Features.Bookings.Queries.GetCustomerBookings;
using PetCareBooking.Application.Features.Bookings.Queries.GetStaffGroomingTasks;
using PetCareBooking.Application.Features.Bookings.Queries.SearchBookings;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BookingsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingCommand command)
        {
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetBookingById(Guid id)
        {
            var response = await _mediator.Send(new GetBookingByIdQuery { Id = id });
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Get customer''s bookings with optional filtering
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
        /// Get assigned grooming tasks for a specific staff member
        /// </summary>
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

        [HttpPut("{id:guid}/cancel")]
        public async Task<IActionResult> CancelBooking(Guid id, [FromBody] CancelBookingCommand command)
        {
            command.Id = id;
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }
    }
}
