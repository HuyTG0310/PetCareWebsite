using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Pet;
using PetCareBooking.Application.Features.Pets.Commands.CreatePet;
using PetCareBooking.Application.Features.Pets.Commands.DeletePet;
using PetCareBooking.Application.Features.Pets.Commands.UpdatePet;
using PetCareBooking.Application.Features.Pets.Queries.GetPetById;
using PetCareBooking.Application.Features.Pets.Queries.GetPetsByCustomerId;
using PetCareBooking.Application.Features.Pets.Queries.GetPetsWithPagination;
using PetCareBooking.Application.Features.Pets.Queries.SearchPets;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PetCareBooking.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PetsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PetsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Lấy danh sách thú cưng của khách hàng đang đăng nhập (Tự đọc từ JWT Token)
        /// </summary>
        [Authorize]
        [HttpGet("my-pets")]
        public async Task<IActionResult> GetMyPets([FromQuery] bool? isActive = null)
        {
            var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                               ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (!Guid.TryParse(customerIdClaim, out var customerId))
            {
                return Unauthorized(new ApiResponse<List<PetResponseDTO>>
                {
                    IsSuccess = false,
                    StatusCode = 401,
                    Message = "Invalid user token.",
                    Result = null
                });
            }

            var response = await _mediator.Send(new GetPetsByCustomerIdQuery
            {
                CustomerId = customerId,
                IsActive = isActive
            });
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Lấy danh sách thú cưng của một khách hàng cụ thể theo CustomerId
        /// </summary>
        [HttpGet("customer/{customerId:guid}")]
        public async Task<IActionResult> GetPetsByCustomerId(Guid customerId, [FromQuery] bool? isActive = null)
        {
            var response = await _mediator.Send(new GetPetsByCustomerIdQuery
            {
                CustomerId = customerId,
                IsActive = isActive
            });
            return StatusCode(response.StatusCode, response);
        }


        [HttpGet]
        public async Task<ActionResult<PagedResult<PetListResponseDTO>>> GetPets([FromQuery] GetPetsWithPaginationQuery query)
        {
            var response = await _mediator.Send(query);
            return Ok(response);
        }

 
        [HttpGet("search")]
        public async Task<ActionResult<PagedResult<PetListResponseDTO>>> SearchPets([FromQuery] SearchPetsQuery query)
        {
            var response = await _mediator.Send(query);
            return Ok(response);
        }

      
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetPetById(Guid id)
        {
            var response = await _mediator.Send(new GetPetByIdQuery { Id = id });
            return StatusCode(response.StatusCode, response);
        }

 
        [HttpPost]
        public async Task<IActionResult> CreatePet([FromBody] CreatePetCommand command)
        {
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }


        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdatePet(Guid id, [FromBody] UpdatePetCommand command)
        {
            command.Id = id;
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeletePet(Guid id)
        {
            var response = await _mediator.Send(new DeletePetCommand { Id = id });
            return StatusCode(response.StatusCode, response);
        }
    }
}
