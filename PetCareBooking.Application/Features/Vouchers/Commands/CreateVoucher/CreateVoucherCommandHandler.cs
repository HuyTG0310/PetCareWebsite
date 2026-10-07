using MediatR;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.Vouchers.Commands.CreateVoucher
{
    public class CreateVoucherCommandHandler : IRequestHandler<CreateVoucherCommand, ApiResponse<Guid>>
    {
        private readonly IGenericRepository<Voucher> _voucherRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateVoucherCommandHandler(IGenericRepository<Voucher> voucherRepository, IUnitOfWork unitOfWork)
        {
            _voucherRepository = voucherRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<Guid>> Handle(CreateVoucherCommand request, CancellationToken cancellationToken)
        {
            var normalizedCode = request.Code.Trim().ToUpper();

            var isDuplicate = await _voucherRepository.GetQueryable()
                .AnyAsync(v => v.Code.ToUpper() == normalizedCode, cancellationToken);

            if (isDuplicate)
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = $"Voucher with code '{normalizedCode}' already exists.",
                    Result = Guid.Empty
                };
            }

            var voucher = new Voucher
            {
                Id = Guid.NewGuid(),
                Code = normalizedCode,
                DiscountType = request.DiscountType,
                DiscountValue = request.DiscountValue,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                MaxUsage = request.MaxUsage,
                CurrentUsage = 0,
                CreatedAt = DateTime.UtcNow
            };

            await _voucherRepository.AddAsync(voucher);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<Guid>
            {
                IsSuccess = true,
                StatusCode = 201,
                Message = "Create voucher successfully.",
                Result = voucher.Id
            };
        }
    }
}

