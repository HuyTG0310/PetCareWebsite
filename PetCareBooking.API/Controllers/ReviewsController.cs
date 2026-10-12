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

        /// <summary>
        /// UC-8.1: View review list (Public for Guest, Customer, Staff, Admin)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetReviews([FromQuery] GetReviewsWithPaginationQuery query)
        {
            var response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// UC-8.2: View review detail
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetReviewById(Guid id)
        {
            var response = await _mediator.Send(new GetReviewByIdQuery(id));
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// UC-8.3: Add review (Requires Customer authentication)
        /// </summary>
        [Authorize(Roles = "Customer")]
        [HttpPost]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewCommand command)
        {
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// UC-8.4: Edit review (Allowed for review owner OR Admin/Staff)
        /// </summary>
        [Authorize]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateReview(Guid id, [FromBody] UpdateReviewCommand command)
        {
            command.Id = id;
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// UC-8.5: Delete review (Allowed for review owner OR Admin/Staff)
        /// </summary>
        [Authorize]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteReview(Guid id)
        {
            var response = await _mediator.Send(new DeleteReviewCommand(id));
            return StatusCode(response.StatusCode, response);
        }
    }
}