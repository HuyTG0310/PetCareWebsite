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
                    Message = "Không tìm thấy tài khoản với email này."
                };
            }

            if (!customer.IsActive)
            {
                return new ApiResponse<string>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Tài khoản đang bị khóa hoặc chưa được kích hoạt."
                };
            }

            // Tạo mã OTP 6 số mới và hết hạn sau 5 phút
            var otpCode = Random.Shared.Next(100000, 999999).ToString();
            customer.OtpCode = otpCode;
            customer.OtpExpiry = DateTime.UtcNow.AddMinutes(5);

            _repository.Update(customer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Gửi OTP qua Email
            await _emailService.SendOtpEmailAsync(customer.Email, otpCode, cancellationToken);

            return new ApiResponse<string>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Mã xác thực đặt lại mật khẩu đã được gửi đến email của bạn.",
                Result = customer.Email
            };
        }
    }
}