using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCareBooking.API.Attributes;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.CareRecord;
using PetCareBooking.Application.Features.CareRecords.Commands.CreateCareRecord;
using PetCareBooking.Application.Features.CareRecords.Commands.DeleteCareRecord;
using PetCareBooking.Application.Features.CareRecords.Commands.UpdateCareRecord;
using PetCareBooking.Application.Features.CareRecords.Queries.GetCareRecordById;
using PetCareBooking.Application.Features.CareRecords.Queries.GetCareRecordsByBookingItem;
using PetCareBooking.Application.Features.CareRecords.Queries.GetCareRecordsWithPagination;
using PetCareBooking.Application.Features.CareRecords.Queries.SearchCareRecords;

namespace PetCareBooking.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CareRecordsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CareRecordsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// [Staff / Admin] Get paginated list of care records system-wide
        /// </summary>
        [RequirePermission("CARE_RECORD_VIEW")]
        [HttpGet]
        public async Task<ActionResult<PagedResult<CareRecordListResponseDTO>>> GetCareRecords([FromQuery] GetCareRecordsWithPaginationQuery query)
        {
            var response = await _mediator.Send(query);
            return Ok(response);
        }

        /// <summary>
        /// [Staff / Admin] Search and filter care records by keyword, staff, or booking item
        /// </summary>
        [RequirePermission("CARE_RECORD_VIEW")]
        [HttpGet("search")]
        public async Task<ActionResult<PagedResult<CareRecordListResponseDTO>>> SearchCareRecords([FromQuery] SearchCareRecordsQuery query)
        {
            var response = await _mediator.Send(query);
            return Ok(response);
        }

        /// <summary>
        /// [Customer / Staff / Admin] Get full care record timeline of a specific boarding service (with IDOR prevention)
        /// </summary>
        [HttpGet("booking-item/{bookingItemId:guid}")]
        public async Task<IActionResult> GetCareRecordsByBookingItem(Guid bookingItemId)
        {
            var response = await _mediator.Send(new GetCareRecordsByBookingItemQuery(bookingItemId));
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// [Customer / Staff / Admin] View details of a care record by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetCareRecordById(Guid id)
        {
            var response = await _mediator.Send(new GetCareRecordByIdQuery { Id = id });
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// [Boarding Staff / Receptionist / Admin] Create a new care record for a boarding pet (auto-detect StaffId with IDOR protection)
        /// </summary>
        [RequirePermission("CARE_RECORD_CREATE")]
        [HttpPost]
        public async Task<IActionResult> CreateCareRecord([FromBody] CreateCareRecordCommand command)
        {
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// [Boarding Staff / Receptionist / Admin] Update a care record
        /// </summary>
        [RequirePermission("CARE_RECORD_MANAGE")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateCareRecord(Guid id, [FromBody] UpdateCareRecordCommand command)
        {
            command.Id = id;
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// [Admin / Receptionist] Delete a care record
        /// </summary>
        [RequirePermission("CARE_RECORD_MANAGE")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteCareRecord(Guid id)
        {
            var response = await _mediator.Send(new DeleteCareRecordCommand { Id = id });
            return StatusCode(response.StatusCode, response);
        }
    }
}
