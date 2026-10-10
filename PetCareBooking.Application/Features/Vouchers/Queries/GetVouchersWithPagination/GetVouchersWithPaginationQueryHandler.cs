using MediatR;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Voucher;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.Vouchers.Queries.GetVouchersWithPagination
{
    public class GetVouchersWithPaginationQueryHandler : IRequestHandler<GetVouchersWithPaginationQuery, PagedResult<VoucherListResponseDTO>>
    {
        private readonly IGenericRepository<Voucher> _voucherRepository;

        public GetVouchersWithPaginationQueryHandler(IGenericRepository<Voucher> voucherRepository)
        {
            _voucherRepository = voucherRepository;
        }

        public async Task<PagedResult<VoucherListResponseDTO>> Handle(GetVouchersWithPaginationQuery request, CancellationToken cancellationToken)
        {
            int pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
            int pageSize = request.PageSize < 1 ? 10 : (request.PageSize > 100 ? 100 : request.PageSize);

            var now = DateTime.UtcNow;
            var query = _voucherRepository.GetQueryable().AsNoTracking();

            int totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(v => v.CreatedAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(v => new VoucherListResponseDTO
                {
                    Id = v.Id,
                    Code = v.Code,
                    DiscountType = v.DiscountType,
                    DiscountValue = v.DiscountValue,
                    StartDate = v.StartDate,
                    EndDate = v.EndDate,
                    MaxUsage = v.MaxUsage,
                    CurrentUsage = v.CurrentUsage,
                    IsValid = now <= v.EndDate && (!v.MaxUsage.HasValue || v.CurrentUsage < v.MaxUsage.Value)
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<VoucherListResponseDTO>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageIndex,
                PageSize = pageSize
            };
        }
    }
}
