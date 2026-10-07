using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.Auth.Commands.ForgotPassword
{
    public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, ApiResponse<string>>
    {
        private readonly IGenericRepository<Customer> _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;

        public ForgotPasswordCommandHandler(
            IGenericRepository<Customer> repository,
            IUnitOfWork unitOfWork,
            IEmailService emailService)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }

        public async Task<ApiResponse<string>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var customer = await _repository.GetQueryable()
                .FirstOrDefaultAsync(c => c.Email == request.Email.Trim().ToLower(), cancellationToken);

            if (customer == null)
            {
                return new ApiResponse<string>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Account with this email was not found."
                };
            }

            if (!customer.IsActive)
            {
                return new ApiResponse<string>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Account is locked or not activated."
                };
            }

            var otpCode = Random.Shared.Next(100000, 999999).ToString();
            customer.OtpCode = otpCode;
            customer.OtpExpiry = DateTime.UtcNow.AddMinutes(5);

            _repository.Update(customer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _emailService.SendOtpEmailAsync(customer.Email, otpCode, cancellationToken);

            return new ApiResponse<string>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Password reset verification code has been sent to your email.",
                Result = customer.Email
            };
        }
    }
}