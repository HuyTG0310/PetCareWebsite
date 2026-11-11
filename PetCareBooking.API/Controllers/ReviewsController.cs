using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCareBooking.Application.Features.Reviews.Queries.GetReviewById;
using PetCareBooking.Application.Features.Reviews.Queries.GetReviewsWithPagination;
using PetCareBooking.Application.Features.Reviews.Commands.CreateReview;
using PetCareBooking.Application.Features.Reviews.Commands.DeleteReview;
using PetCareBooking.Application.Features.Reviews.Commands.UpdateReview;

namespace PetCareBooking.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReviewsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetReviews([FromQuery] GetReviewsWithPaginationQuery query)
        {
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetReviewById(Guid id)
        {
            var response = await _mediator.Send(new GetReviewByIdQuery(id));
            return StatusCode(response.StatusCode, response);
        }

        [Authorize(Roles = "Customer")]
        [HttpPost]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewCommand command)
        {
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        [Authorize]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateReview(Guid id, [FromBody] UpdateReviewCommand command)
        {
            command.Id = id;
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        [Authorize]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteReview(Guid id)
        {
            var response = await _mediator.Send(new DeleteReviewCommand(id));
            return StatusCode(response.StatusCode, response);
        }
    }
}