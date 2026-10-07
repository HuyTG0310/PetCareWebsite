using MediatR;
using Microsoft.AspNetCore.Mvc;
using PetCareBooking.Application.Features.Customers.Commands.CreateCustomer;
using PetCareBooking.Application.Features.Customers.Commands.DeleteCustomer;
using PetCareBooking.Application.Features.Customers.Commands.ToggleCustomerStatus;
using PetCareBooking.Application.Features.Customers.Commands.UpdateCustomer;
using PetCareBooking.Application.Features.Customers.Queries.GetCustomerById;
using PetCareBooking.Application.Features.Customers.Queries.GetCustomersWithPagination;

namespace PetCareBooking.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CustomersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Get customer list & Search customers
        [HttpGet]
        public async Task<IActionResult> GetCustomers([FromQuery] GetCustomersWithPaginationQuery query)
        {
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        // View customer profile details & history
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetCustomerById(Guid id)
        {
            var response = await _mediator.Send(new GetCustomerByIdQuery { Id = id });
            return StatusCode(response.StatusCode, response);
        }

        // Add new customer
        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerCommand command)
        {
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        // Update customer information
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateCustomer(Guid id, [FromBody] UpdateCustomerCommand command)
        {
            command.Id = id;
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        // Soft delete / Deactivate customer
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteCustomer(Guid id)
        {
            var response = await _mediator.Send(new DeleteCustomerCommand { Id = id });
            return StatusCode(response.StatusCode, response);
        }

        // Toggle active status
        [HttpPatch("{id:guid}/toggle-status")]
        public async Task<IActionResult> ToggleStatus(Guid id)
        {
            var response = await _mediator.Send(new ToggleCustomerStatusCommand { Id = id });
            return StatusCode(response.StatusCode, response);
        }
    }
}