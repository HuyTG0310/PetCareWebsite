using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.Vouchers.Commands.UpdateVoucher
{
    public class UpdateVoucherCommandHandler : IRequestHandler<UpdateVoucherCommand, ApiResponse<Guid>>
    {
        private readonly IGenericRepository<Voucher> _voucherRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateVoucherCommandHandler(IGenericRepository<Voucher> voucherRepository, IUnitOfWork unitOfWork)
        {
            _voucherRepository = voucherRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<Guid>> Handle(UpdateVoucherCommand request, CancellationToken cancellationToken)
        {
            var voucher = await _voucherRepository.GetByIdAsync(request.Id);
            if (voucher == null)
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"Voucher with ID {request.Id} not found.",
                    Result = Guid.Empty
                };
            }

            var normalizedCode = request.Code.Trim().ToUpper();
            var isDuplicate = await _voucherRepository.GetQueryable()
                .AnyAsync(v => v.Code.ToUpper() == normalizedCode && v.Id != request.Id, cancellationToken);

            if (isDuplicate)
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = $"Voucher with code '{normalizedCode}' already exists.",
                    Result = Guid.Empty
                };
            }

            voucher.Code = normalizedCode;
            voucher.DiscountType = request.DiscountType;
            voucher.DiscountValue = request.DiscountValue;
            voucher.StartDate = request.StartDate;
            voucher.EndDate = request.EndDate;
            voucher.MaxUsage = request.MaxUsage;
            voucher.UpdatedAt = DateTime.UtcNow;

            _voucherRepository.Update(voucher);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<Guid>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Update voucher successfully.",
                Result = voucher.Id
            };
        }
    }
}
