using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Review;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.Reviews.Queries.GetReviewsWithPagination
{
    public class GetReviewsWithPaginationQueryHandler 
        : IRequestHandler<GetReviewsWithPaginationQuery, ApiResponse<PagedResult<ReviewListResponseDto>>>
    {
        private readonly IGenericRepository<Review> _repository;

        public GetReviewsWithPaginationQueryHandler(IGenericRepository<Review> repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<PagedResult<ReviewListResponseDto>>> Handle(
            GetReviewsWithPaginationQuery request, CancellationToken cancellationToken)
        {
            var query = _repository.GetQueryable().AsNoTracking();

            if (request.ServiceId.HasValue)
            {
                query = query.Where(r => r.ServiceId == request.ServiceId.Value);
            }

            if (request.Rating.HasValue)
            {
                query = query.Where(r => r.Rating == request.Rating.Value);
            }

            int totalCount = await query.CountAsync(cancellationToken);

            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

            var items = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new ReviewListResponseDto
                {
                    Id = r.Id,
                    ServiceId = r.ServiceId,
                    ServiceName = r.Service.Name,
                    CustomerId = r.CustomerId,
                    CustomerName = r.Customer.FullName,
                    CustomerAvatarUrl = r.Customer.AvatarUrl,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                })
                .ToListAsync(cancellationToken);

            return new ApiResponse<PagedResult<ReviewListResponseDto>>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Review list retrieved successfully.",
                Result = new PagedResult<ReviewListResponseDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                }
            };
        }
    }
}
