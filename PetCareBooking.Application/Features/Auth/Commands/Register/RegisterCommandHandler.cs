using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.Auth.Commands.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, ApiResponse<string>>
    {
        private readonly IGenericRepository<Customer> _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;

        public RegisterCommandHandler(
            IGenericRepository<Customer> repository,
            IUnitOfWork unitOfWork,
            IEmailService emailService)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }

        public async Task<ApiResponse<string>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            // Check if email already exists regardless of active or inactive status
            var emailExists = await _repository.GetQueryable()
                .AnyAsync(c => c.Email == request.Email, cancellationToken);

            if (emailExists)
            {
                return new ApiResponse<string>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Email already exists in the system."
                };
            }

            // Check if phone number already exists regardless of active or inactive status
            var phoneExists = await _repository.GetQueryable()
                .AnyAsync(c => c.PhoneNumber == request.PhoneNumber, cancellationToken);

            if (phoneExists)
            {
                return new ApiResponse<string>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Phone number already exists in the system."
                };
            }

            // Create new unactivated account
            var otpCode = Random.Shared.Next(100000, 999999).ToString();
            var customer = new Customer
            {
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                IsActive = false,
                OtpCode = otpCode,
                OtpExpiry = DateTime.UtcNow.AddMinutes(5)
            };

            await _repository.AddAsync(customer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _emailService.SendOtpEmailAsync(customer.Email, otpCode, cancellationToken);

            return new ApiResponse<string>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status201Created,
                Message = "Registration successful! Please check your email for the OTP code.",
                Result = customer.Email
            };
        }
    }
}