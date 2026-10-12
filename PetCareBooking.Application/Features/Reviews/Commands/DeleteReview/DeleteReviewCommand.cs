using MediatR;
using PetCareBooking.Application.Common.Models;

namespace PetCareBooking.Application.Features.Reviews.Commands.DeleteReview
{
    public class DeleteReviewCommand : IRequest<ApiResponse<bool>>
    {
        public Guid Id { get; set; }

        public DeleteReviewCommand(Guid id)
        {
            Id = id;
        }
    }
}
