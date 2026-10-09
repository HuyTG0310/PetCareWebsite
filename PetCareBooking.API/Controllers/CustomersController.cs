using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCareBooking.Application.Features.Customers.Command.ChangePassword;
using PetCareBooking.Application.Features.Customers.Command.UpdateProfile;
using PetCareBooking.Application.Features.Customers.Commands.CreateCustomer;
using PetCareBooking.Application.Features.Customers.Commands.DeleteCustomer;
using PetCareBooking.Application.Features.Customers.Commands.ToggleCustomerStatus;
using PetCareBooking.Application.Features.Customers.Commands.UpdateCustomer;
using PetCareBooking.Application.Features.Customers.Queries.GetCustomerById;
using PetCareBooking.Application.Features.Customers.Queries.GetCustomersWithPagination;
using PetCareBooking.Application.Features.Customers.Queries.GetProfile;

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

        /// <summary>
        /// Get customer list & Search customers (Admin/Staff)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCustomers([FromQuery] GetCustomersWithPaginationQuery query)
        {
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// View customer profile details & history (Admin/Staff)
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetCustomerById(Guid id)
        {
            var response = await _mediator.Send(new GetCustomerByIdQuery { Id = id });
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Add new customer (Admin/Staff)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerCommand command)
        {
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Update customer information by ID (Admin/Staff)
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateCustomer(Guid id, [FromBody] UpdateCustomerCommand command)
        {
            command.Id = id;
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Soft delete / Deactivate customer (Admin/Staff)
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteCustomer(Guid id)
        {
            var response = await _mediator.Send(new DeleteCustomerCommand { Id = id });
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Toggle active status of customer (Admin/Staff)
        /// </summary>
        [HttpPatch("{id:guid}/toggle-status")]
        public async Task<IActionResult> ToggleStatus(Guid id)
        {
            var response = await _mediator.Send(new ToggleCustomerStatusCommand { Id = id });
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Get profile information of the authenticated customer
        /// </summary>
        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var response = await _mediator.Send(new GetCustomerProfileQuery());
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Update profile and avatar for the authenticated customer
        /// </summary>
        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateCustomerProfileCommand command)
        {
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Change password for the authenticated customer
        /// </summary>
        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
        {
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }
    }
}