using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.Auth.Commands.ResetPassword
{
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, ApiResponse<bool>>
    {
        private readonly IGenericRepository<Customer> _repository;
        private readonly IUnitOfWork _unitOfWork;

        public ResetPasswordCommandHandler(IGenericRepository<Customer> repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<bool>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var customer = await _repository.GetQueryable()
                .FirstOrDefaultAsync(c => c.Email == request.Email.Trim().ToLower(), cancellationToken);

            if (customer == null)
            {
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Corresponding account not found."
                };
            }

            if (string.IsNullOrEmpty(customer.OtpCode) ||
                customer.OtpCode != request.OtpCode.Trim() ||
                customer.OtpExpiry < DateTime.UtcNow)
            {
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Invalid or expired OTP code."
                };
            }

            customer.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            customer.OtpCode = null;
            customer.OtpExpiry = null;
            customer.UpdatedAt = DateTime.UtcNow;

            _repository.Update(customer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<bool>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Password reset successful! You can now log in with your new password.",
                Result = true
            };
        }
    }
}