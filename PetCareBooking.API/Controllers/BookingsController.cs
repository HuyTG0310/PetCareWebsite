using MediatR;
using Microsoft.AspNetCore.Mvc;
using PetCareBooking.Application.Features.Bookings.Commands.CancelBooking;
using PetCareBooking.Application.Features.Bookings.Commands.CreateBooking;
using PetCareBooking.Application.Features.Bookings.Commands.UpdateBookingItemStatus;
using PetCareBooking.Application.Features.Bookings.Queries.GetAdminBookings;
using PetCareBooking.Application.Features.Bookings.Queries.GetBookingById;
using PetCareBooking.Application.Features.Bookings.Queries.GetCustomerBookings;
using PetCareBooking.Application.Features.Bookings.Queries.SearchBookings;

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
        /// Get customer's bookings with optional filtering
        /// </summary>
        [HttpGet("customer/{customerId:guid}")]
        public async Task<IActionResult> GetCustomerBookings(
            Guid customerId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? status = null)
        {
            var query = new GetCustomerBookingsQuery
            {
                CustomerId = customerId,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Status = status.HasValue ? (Domain.Enums.BookingStatus)status.Value : null
            };
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("admin")]
        public async Task<IActionResult> GetAdminBookings(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? status = null,
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null)
        {
            var query = new GetAdminBookingsQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Status = status.HasValue ? (Domain.Enums.BookingStatus)status.Value : null,
                DateFrom = dateFrom,
                DateTo = dateTo
            };
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchBookings(
            [FromQuery] string? keyword = null,
            [FromQuery] int? status = null,
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new SearchBookingsQuery
            {
                Keyword = keyword,
                Status = status.HasValue ? (Domain.Enums.BookingStatus)status.Value : null,
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