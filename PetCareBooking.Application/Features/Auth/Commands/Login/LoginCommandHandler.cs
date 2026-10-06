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
            // 1. Tìm thông tin khách hàng theo Email
            var customer = await _repository.GetQueryable()
                .FirstOrDefaultAsync(c => c.Email == request.Email, cancellationToken);

            if (customer == null)
            {
                return new ApiResponse<LoginResponseDto>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Email hoặc mật khẩu không chính xác."
                };
            }

            // 2. Kiểm tra mật khẩu mã hóa BCrypt
            var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, customer.PasswordHash);
            if (!isPasswordValid)
            {
                return new ApiResponse<LoginResponseDto>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Email hoặc mật khẩu không chính xác."
                };
            }

            // 3. Kiểm tra tài khoản đã xác thực OTP chưa
            if (!customer.IsActive)
            {
                return new ApiResponse<LoginResponseDto>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Tài khoản chưa được kích hoạt. Vui lòng xác thực mã OTP trước khi đăng nhập."
                };
            }

            // 4. Sinh JWT Token
            var token = _jwtTokenGenerator.GenerateToken(customer);

            // 5. Trả về thông tin
            return new ApiResponse<LoginResponseDto>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Đăng nhập thành công!",
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