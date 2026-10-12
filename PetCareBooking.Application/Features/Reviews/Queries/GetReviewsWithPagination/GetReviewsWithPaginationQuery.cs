using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Review;

namespace PetCareBooking.Application.Features.Reviews.Queries.GetReviewsWithPagination
{
    public class GetReviewsWithPaginationQuery : IRequest<ApiResponse<PagedResult<ReviewListResponseDto>>>
    {
        public Guid? ServiceId { get; set; }
        public int? Rating { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}