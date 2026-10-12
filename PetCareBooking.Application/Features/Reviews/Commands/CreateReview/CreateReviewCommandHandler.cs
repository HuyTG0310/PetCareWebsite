using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.Features.Reviews.Commands.CreateReview
{
    public class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, ApiResponse<Guid>>
    {
        private readonly IGenericRepository<Review> _reviewRepository;
        private readonly IGenericRepository<BookingItem> _bookingItemRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;

        public CreateReviewCommandHandler(
            IGenericRepository<Review> reviewRepository,
            IGenericRepository<BookingItem> bookingItemRepository,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork)
        {
            _reviewRepository = reviewRepository;
            _bookingItemRepository = bookingItemRepository;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<Guid>> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
        {
            var customerId = _currentUserService.UserId;
            if (customerId == null)
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Unauthorized access."
                };
            }

            // 1. Check if BookingItem exists and belongs to the current Customer
            var bookingItem = await _bookingItemRepository.GetQueryable()
                .Include(bi => bi.Booking)
                .FirstOrDefaultAsync(bi => bi.Id == request.BookingItemId && bi.Booking.CustomerId == customerId.Value, cancellationToken);

            if (bookingItem == null)
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Booking item not found or does not belong to you."
                };
            }

            // 2. Only allow review when the service is Completed
            if (bookingItem.Status != BookingItemStatus.Completed)
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "You can only review a completed service."
                };
            }

            // 3. Check if this BookingItem has already been reviewed
            var isReviewed = await _reviewRepository.GetQueryable()
                .AnyAsync(r => r.BookingItemId == request.BookingItemId, cancellationToken);

            if (isReviewed)
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "This booking item has already been reviewed."
                };
            }

            var review = new Review
            {
                BookingItemId = bookingItem.Id,
                ServiceId = bookingItem.ServiceId,
                CustomerId = customerId.Value,
                Rating = request.Rating,
                Comment = request.Comment,
                CreatedAt = DateTime.UtcNow
            };

            await _reviewRepository.AddAsync(review);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<Guid>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status201Created,
                Message = "Review submitted successfully.",
                Result = review.Id
            };
        }
    }
}