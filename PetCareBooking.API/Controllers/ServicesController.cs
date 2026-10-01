using MediatR;
using Microsoft.AspNetCore.Mvc;
using PetCareBooking.Application.Features.Services.Commands.CreateService;
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
        public async Task<IActionResult> GetAllServices()
        {
            var query = new GetAllServicesQuery();
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }


        [HttpPost]
        public async Task<IActionResult> CreateService([FromBody] CreateServiceCommand command)
        {
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetServiceById(Guid id)
        {
            var query = new GetServiceByIdQuery(id);
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }
    }
}
