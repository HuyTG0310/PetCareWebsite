using MediatR;
using PetCareBooking.Application.Common.Models;

namespace PetCareBooking.Application.Features.Reviews.Commands.CreateReview
{
    public class CreateReviewCommand : IRequest<ApiResponse<Guid>>
    {
        public Guid BookingItemId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}