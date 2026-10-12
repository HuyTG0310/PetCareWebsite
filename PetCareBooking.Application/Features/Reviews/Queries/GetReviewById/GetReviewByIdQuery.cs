using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Review;

namespace PetCareBooking.Application.Features.Reviews.Queries.GetReviewById
{
    public class GetReviewByIdQuery : IRequest<ApiResponse<ReviewDetailResponseDto>>
    {
        public Guid Id { get; set; }

        public GetReviewByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}