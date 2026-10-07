using MediatR;
using Microsoft.AspNetCore.Mvc;
using PetCareBooking.Application.Features.Vouchers.Commands.CreateVoucher;

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

        /// <summary>
        /// Tạo mới voucher giảm giá (Khuyến mãi)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateVoucher([FromBody] CreateVoucherCommand command)
        {
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }
    }
}

