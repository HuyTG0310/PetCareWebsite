using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Review;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.Reviews.Queries.GetReviewById
{
    public class GetReviewByIdQueryHandler : IRequestHandler<GetReviewByIdQuery, ApiResponse<ReviewDetailResponseDto>>
    {
        private readonly IGenericRepository<Review> _repository;

        public GetReviewByIdQueryHandler(IGenericRepository<Review> repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<ReviewDetailResponseDto>> Handle(GetReviewByIdQuery request, CancellationToken cancellationToken)
        {
            var review = await _repository.GetQueryable()
                .AsNoTracking()
                .Where(r => r.Id == request.Id)
                .Select(r => new ReviewDetailResponseDto
                {
                    Id = r.Id,
                    BookingItemId = r.BookingItemId,
                    ServiceId = r.ServiceId,
                    ServiceName = r.Service.Name,
                    CustomerId = r.CustomerId,
                    CustomerName = r.Customer.FullName,
                    CustomerEmail = r.Customer.Email,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (review == null)
            {
                return new ApiResponse<ReviewDetailResponseDto>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Review not found."
                };
            }

            return new ApiResponse<ReviewDetailResponseDto>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Review detail retrieved successfully.",
                Result = review
            };
        }
    }
}