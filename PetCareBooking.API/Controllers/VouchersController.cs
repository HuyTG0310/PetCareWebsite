using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCareBooking.API.Attributes;
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
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class VouchersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public VouchersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [RequirePermission("VOUCHER_MANAGE")]
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

        [RequirePermission("VOUCHER_VIEW")]
        [HttpGet]
        public async Task<ActionResult<PagedResult<VoucherListResponseDTO>>> GetVouchers([FromQuery] GetVouchersWithPaginationQuery query)
        {
            var response = await _mediator.Send(query);
            return Ok(response);
        }

        [RequirePermission("VOUCHER_VIEW")]
        [HttpGet("search")]
        public async Task<ActionResult<PagedResult<VoucherListResponseDTO>>> SearchVouchers([FromQuery] SearchVouchersQuery query)
        {
            var response = await _mediator.Send(query);
            return Ok(response);
        }

        [RequirePermission("VOUCHER_VIEW")]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetVoucherById(Guid id)
        {
            var response = await _mediator.Send(new GetVoucherByIdQuery { Id = id });
            return StatusCode(response.StatusCode, response);
        }

        [RequirePermission("VOUCHER_MANAGE")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateVoucher(Guid id, [FromBody] UpdateVoucherCommand command)
        {
            command.Id = id;
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        [RequirePermission("VOUCHER_MANAGE")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteVoucher(Guid id)
        {
            var response = await _mediator.Send(new DeleteVoucherCommand { Id = id });
            return StatusCode(response.StatusCode, response);
        }
    }
}
