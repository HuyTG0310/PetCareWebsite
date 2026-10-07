using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.Auth.Commands.ResendOtp
{
    public class ResendOtpCommandHandler : IRequestHandler<ResendOtpCommand, ApiResponse<bool>>
    {
        private readonly IGenericRepository<Customer> _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;

        public ResendOtpCommandHandler(
            IGenericRepository<Customer> repository,
            IUnitOfWork unitOfWork,
            IEmailService emailService)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }

        public async Task<ApiResponse<bool>> Handle(ResendOtpCommand request, CancellationToken cancellationToken)
        {
            var customer = await _repository.GetQueryable()
                .FirstOrDefaultAsync(c => c.Email == request.Email, cancellationToken);

            if (customer == null)
            {
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "No account found corresponding to this email."
                };
            }

            if (customer.IsActive)
            {
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Account is already activated. You can log in directly."
                };
            }

            var newOtp = Random.Shared.Next(100000, 999999).ToString();
            customer.OtpCode = newOtp;
            customer.OtpExpiry = DateTime.UtcNow.AddMinutes(5);

            _repository.Update(customer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _emailService.SendOtpEmailAsync(customer.Email, newOtp, cancellationToken);

            return new ApiResponse<bool>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "A new OTP code has been sent to your email.",
                Result = true
            };
        }
    }
}