using MediatR;
using Microsoft.AspNetCore.Mvc;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.CareRecord;
using PetCareBooking.Application.Features.CareRecords.Commands.CreateCareRecord;
using PetCareBooking.Application.Features.CareRecords.Commands.DeleteCareRecord;
using PetCareBooking.Application.Features.CareRecords.Commands.UpdateCareRecord;
using PetCareBooking.Application.Features.CareRecords.Queries.GetCareRecordById;
using PetCareBooking.Application.Features.CareRecords.Queries.GetCareRecordsWithPagination;
using PetCareBooking.Application.Features.CareRecords.Queries.SearchCareRecords;

namespace PetCareBooking.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CareRecordsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CareRecordsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<CareRecordListResponseDTO>>> GetCareRecords([FromQuery] GetCareRecordsWithPaginationQuery query)
        {
            var response = await _mediator.Send(query);
            return Ok(response);
        }

        [HttpGet("search")]
        public async Task<ActionResult<PagedResult<CareRecordListResponseDTO>>> SearchCareRecords([FromQuery] SearchCareRecordsQuery query)
        {
            var response = await _mediator.Send(query);
            return Ok(response);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetCareRecordById(Guid id)
        {
            var response = await _mediator.Send(new GetCareRecordByIdQuery { Id = id });
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCareRecord([FromBody] CreateCareRecordCommand command)
        {
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateCareRecord(Guid id, [FromBody] UpdateCareRecordCommand command)
        {
            command.Id = id;
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteCareRecord(Guid id)
        {
            var response = await _mediator.Send(new DeleteCareRecordCommand { Id = id });
            return StatusCode(response.StatusCode, response);
        }
    }
}
