using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Voucher;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.Vouchers.Queries.GetVoucherById
{
    public class GetVoucherByIdQueryHandler : IRequestHandler<GetVoucherByIdQuery, ApiResponse<VoucherResponseDTO>>
    {
        private readonly IGenericRepository<Voucher> _voucherRepository;

        public GetVoucherByIdQueryHandler(IGenericRepository<Voucher> voucherRepository)
        {
            _voucherRepository = voucherRepository;
        }

        public async Task<ApiResponse<VoucherResponseDTO>> Handle(GetVoucherByIdQuery request, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            var voucher = await _voucherRepository.GetQueryable()
                .AsNoTracking()
                .Where(v => v.Id == request.Id)
                .Select(v => new VoucherResponseDTO
                {
                    Id = v.Id,
                    Code = v.Code,
                    DiscountType = v.DiscountType,
                    DiscountValue = v.DiscountValue,
                    StartDate = v.StartDate,
                    EndDate = v.EndDate,
                    MaxUsage = v.MaxUsage,
                    CurrentUsage = v.CurrentUsage,
                    IsValid = now <= v.EndDate && (!v.MaxUsage.HasValue || v.CurrentUsage < v.MaxUsage.Value),
                    CreatedAt = v.CreatedAt,
                    UpdatedAt = v.UpdatedAt
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (voucher == null)
            {
                return new ApiResponse<VoucherResponseDTO>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"Voucher with ID {request.Id} not found.",
                    Result = null
                };
            }

            return new ApiResponse<VoucherResponseDTO>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Get voucher detail successfully.",
                Result = voucher
            };
        }
    }
}
