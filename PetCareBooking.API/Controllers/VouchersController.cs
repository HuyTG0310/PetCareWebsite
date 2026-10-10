using MediatR;
using Microsoft.AspNetCore.Mvc;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Voucher;
using PetCareBooking.Application.Features.Vouchers.Commands.CreateVoucher;
using PetCareBooking.Application.Features.Vouchers.Commands.DeleteVoucher;
using PetCareBooking.Application.Features.Vouchers.Commands.UpdateVoucher;
using PetCareBooking.Application.Features.Vouchers.Queries.GetVoucherById;
using PetCareBooking.Application.Features.Vouchers.Queries.GetVouchersWithPagination;
using PetCareBooking.Application.Features.Vouchers.Queries.SearchVouchers;

namespace PetCareBooking.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VouchersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public VouchersController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [HttpPost]
        public async Task<IActionResult> CreateVoucher([FromBody] CreateVoucherCommand command)
        {
            var response = await _mediator.Send(command);
            if (!response.IsSuccess)
            {
                return StatusCode(response.StatusCode, response);
            }

            return CreatedAtAction(nameof(GetVoucherById), new { id = response.Result }, response);
        }

   
        [HttpGet]
        public async Task<ActionResult<PagedResult<VoucherListResponseDTO>>> GetVouchers([FromQuery] GetVouchersWithPaginationQuery query)
        {
            var response = await _mediator.Send(query);
            return Ok(response);
        }

        [HttpGet("search")]
        public async Task<ActionResult<PagedResult<VoucherListResponseDTO>>> SearchVouchers([FromQuery] SearchVouchersQuery query)
        {
            var response = await _mediator.Send(query);
            return Ok(response);
        }

    
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetVoucherById(Guid id)
        {
            var response = await _mediator.Send(new GetVoucherByIdQuery { Id = id });
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateVoucher(Guid id, [FromBody] UpdateVoucherCommand command)
        {
            command.Id = id;
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteVoucher(Guid id)
        {
            var response = await _mediator.Send(new DeleteVoucherCommand { Id = id });
            return StatusCode(response.StatusCode, response);
        }
    }
}
