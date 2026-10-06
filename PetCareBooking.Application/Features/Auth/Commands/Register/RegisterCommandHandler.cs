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
            var existingCustomer = await _repository.GetQueryable()
                .FirstOrDefaultAsync(c => c.Email == request.Email || c.PhoneNumber == request.PhoneNumber, cancellationToken);

            if (existingCustomer != null)
            {
                if (existingCustomer.IsActive)
                {
                    return new ApiResponse<string>
                    {
                        IsSuccess = false,
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "Email hoặc số điện thoại đã tồn tại và đã được kích hoạt."
                    };
                }

                // Nếu tài khoản cũ chưa active: cập nhật thông tin và cấp lại OTP mới
                existingCustomer.FullName = request.FullName;
                existingCustomer.PhoneNumber = request.PhoneNumber;
                existingCustomer.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                existingCustomer.OtpCode = Random.Shared.Next(100000, 999999).ToString();
                existingCustomer.OtpExpiry = DateTime.UtcNow.AddMinutes(5);

                _repository.Update(existingCustomer);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _emailService.SendOtpEmailAsync(existingCustomer.Email, existingCustomer.OtpCode);

                return new ApiResponse<string>
                {
                    IsSuccess = true,
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Mã OTP mới đã được gửi lại tới email của bạn.",
                    Result = existingCustomer.Email
                };
            }

            // Tạo tài khoản mới chưa kích hoạt
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

            await _emailService.SendOtpEmailAsync(customer.Email, otpCode);

            return new ApiResponse<string>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status201Created,
                Message = "Đăng ký thành công! Vui lòng kiểm tra email để lấy mã OTP.",
                Result = customer.Email
            };
        }
    }
}