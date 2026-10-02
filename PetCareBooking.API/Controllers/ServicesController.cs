using MediatR;
using Microsoft.AspNetCore.Mvc;
using PetCareBooking.Application.Features.Services.Commands.CreateService;
using PetCareBooking.Application.Features.Services.Commands.DeleteService;
using PetCareBooking.Application.Features.Services.Commands.UpdateService;
using PetCareBooking.Application.Features.Services.Queries.GetActiveServices;
using PetCareBooking.Application.Features.Services.Queries.GetAllServices;
using PetCareBooking.Application.Features.Services.Queries.GetServiceById;

namespace PetCareBooking.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ServicesController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [HttpGet]
        public async Task<IActionResult> GetActiveServices()
        {
            var response = await _mediator.Send(new GetActiveServicesQuery());
            return StatusCode(response.StatusCode, response);
        }


        [HttpGet("all")]
        public async Task<IActionResult> GetAllServicesForAdmin()
        {
            var response = await _mediator.Send(new GetAllServicesQuery());
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetServiceById(Guid id)
        {
            var response = await _mediator.Send(new GetServiceByIdQuery { Id = id });
            return StatusCode(response.StatusCode, response);
        }


        [HttpPost]
        public async Task<IActionResult> CreateService([FromBody] CreateServiceCommand command)
        {
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateService(Guid id, [FromBody] UpdateServiceCommand command)
        {
            command.Id = id;

            var response = await _mediator.Send(command);

            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteService(Guid id)
        {
            var command = new DeleteServiceCommand { Id = id };

            var response = await _mediator.Send(command);

            return StatusCode(response.StatusCode, response);
        }
    }
}
