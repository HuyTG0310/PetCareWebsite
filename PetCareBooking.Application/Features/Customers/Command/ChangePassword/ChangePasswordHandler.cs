using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.Customers.Command.ChangePassword
{
    public class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, ApiResponse<bool>>
    {
        private readonly IGenericRepository<Customer> _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public ChangePasswordHandler(
            IGenericRepository<Customer> repository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<ApiResponse<bool>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (userId == null)
            {
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Unauthorized access."
                };
            }

            var customer = await _repository.GetQueryable()
                .FirstOrDefaultAsync(c => c.Id == userId.Value, cancellationToken);

            if (customer == null || !customer.IsActive)
            {
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Account is inactive or not found."
                };
            }

            // Verify current password
            var isCurrentPasswordValid = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, customer.PasswordHash);
            if (!isCurrentPasswordValid)
            {
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Current password is incorrect."
                };
            }

            // Reject if new password matches old password
            if (BCrypt.Net.BCrypt.Verify(request.NewPassword, customer.PasswordHash))
            {
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "New password cannot be the same as current password."
                };
            }

            customer.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            customer.UpdatedAt = DateTime.UtcNow;

            _repository.Update(customer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<bool>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Password changed successfully.",
                Result = true
            };
        }
    }
}