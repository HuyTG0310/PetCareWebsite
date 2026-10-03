using MediatR;
using Microsoft.AspNetCore.Mvc;
using PetCareBooking.Application.Features.Services.Commands.CreateService;
using PetCareBooking.Application.Features.Services.Commands.DeleteService;
using PetCareBooking.Application.Features.Services.Commands.UpdateService;
using PetCareBooking.Application.Features.Services.Queries.GetActiveServices;
using PetCareBooking.Application.Features.Services.Queries.GetAllServices;
using PetCareBooking.Application.Features.Services.Queries.GetServiceById;
using PetCareBooking.Application.Features.Services.Queries.SearchServices;
using PetCareBooking.Application.Features.Services.Queries.SearchServicesForAdmin;

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

        // this endpoint is used by customer to view active services
        [HttpGet]
        public async Task<IActionResult> GetActiveServices([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var query = new GetActiveServicesQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }


        // this endpoint is used by customer to search active services
        [HttpGet("search")]
        public async Task<IActionResult> SearchServices([FromQuery] SearchServicesQuery query)
        {
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetServiceById(Guid id)
        {
            var response = await _mediator.Send(new GetServiceByIdQuery { Id = id });
            return StatusCode(response.StatusCode, response);
        }


        // this endpoints is used by admin to view all services include inactive
        [HttpGet("admin")]
        public async Task<IActionResult> GetAllServicesForAdmin([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var query = new GetAllServicesQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }


        // this endpoint is used by admin to search both active and inactive services
        [HttpGet("admin/search")]
        public async Task<IActionResult> SearchServicesForAdmin([FromQuery] SearchServicesForAdminQuery query)
        {
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }


        [HttpPost]
        public async Task<IActionResult> CreateService([FromBody] CreateServiceCommand command)
        {
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateService(Guid id, [FromBody] UpdateServiceCommand command)
        {
            command.Id = id;

            var response = await _mediator.Send(command);

            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteService(Guid id)
        {
            var command = new DeleteServiceCommand { Id = id };

            var response = await _mediator.Send(command);

            return StatusCode(response.StatusCode, response);
        }
    }
}
