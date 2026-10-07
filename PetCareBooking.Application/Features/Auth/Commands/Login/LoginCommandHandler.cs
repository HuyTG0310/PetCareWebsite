using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.Auth.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, ApiResponse<LoginResponseDto>>
    {
        private readonly IGenericRepository<Customer> _repository;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public LoginCommandHandler(IGenericRepository<Customer> repository, IJwtTokenGenerator jwtTokenGenerator)
        {
            _repository = repository;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<ApiResponse<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var customer = await _repository.GetQueryable()
                .FirstOrDefaultAsync(c => c.Email == request.Email, cancellationToken);

            if (customer == null)
            {
                return new ApiResponse<LoginResponseDto>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Invalid email or password."
                };
            }

            var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, customer.PasswordHash);
            if (!isPasswordValid)
            {
                return new ApiResponse<LoginResponseDto>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Invalid email or password."
                };
            }

            if (!customer.IsActive)
            {
                return new ApiResponse<LoginResponseDto>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Account is not activated. Please verify OTP code before logging in."
                };
            }

            var token = _jwtTokenGenerator.GenerateToken(customer);

            return new ApiResponse<LoginResponseDto>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Login successful!",
                Result = new LoginResponseDto
                {
                    Token = token,
                    CustomerId = customer.Id,
                    FullName = customer.FullName,
                    Email = customer.Email,
                    PhoneNumber = customer.PhoneNumber
                }
            };
        }
    }
}