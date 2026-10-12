using MediatR;
using Microsoft.AspNetCore.Http;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.Reviews.Commands.DeleteReview
{
    public class DeleteReviewCommandHandler : IRequestHandler<DeleteReviewCommand, ApiResponse<bool>>
    {
        private readonly IGenericRepository<Review> _repository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteReviewCommandHandler(
            IGenericRepository<Review> repository,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<bool>> Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.UserId;
            var isAdminOrStaff = _currentUserService.IsAdminOrStaff ||
                                 _currentUserService.IsInRole("Admin") ||
                                 _currentUserService.IsInRole("Staff");

            if (currentUserId == null && !isAdminOrStaff)
            {
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Unauthorized access."
                };
            }

            var review = await _repository.GetByIdAsync(request.Id);
            if (review == null)
            {
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Review not found."
                };
            }

            // Authorization: Only the review owner OR Admin/Staff can delete (anti-spam / moderation)
            if (!isAdminOrStaff && review.CustomerId != currentUserId)
            {
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "You do not have permission to delete this review."
                };
            }

            _repository.Delete(review);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<bool>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Review deleted successfully.",
                Result = true
            };
        }
    }
}