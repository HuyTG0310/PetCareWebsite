using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.Auth.Commands.VerifyOtp
{
    public class VerifyOtpCommandHandler : IRequestHandler<VerifyOtpCommand, ApiResponse<bool>>
    {
        private readonly IGenericRepository<Customer> _repository;
        private readonly IUnitOfWork _unitOfWork;

        public VerifyOtpCommandHandler(IGenericRepository<Customer> repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<bool>> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
        {
            var customer = await _repository.GetQueryable()
                .FirstOrDefaultAsync(c => c.Email == request.Email, cancellationToken);

            if (customer == null)
            {
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Account does not exist."
                };
            }

            if (customer.IsActive)
            {
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "This account has already been activated."
                };
            }

            if (customer.OtpCode != request.OtpCode.Trim() || customer.OtpExpiry < DateTime.UtcNow)
            {
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Invalid or expired OTP code."
                };
            }

            customer.IsActive = true;
            customer.OtpCode = null;
            customer.OtpExpiry = null;
            customer.UpdatedAt = DateTime.UtcNow;

            _repository.Update(customer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<bool>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Account verification successful! You can now log in.",
                Result = true
            };
        }
    }
}