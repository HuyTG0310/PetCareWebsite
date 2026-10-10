using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Voucher;

namespace PetCareBooking.Application.Features.Vouchers.Queries.SearchVouchers
{
    public class SearchVouchersQuery : IRequest<PagedResult<VoucherListResponseDTO>>
    {
        public string? Keyword { get; set; }
        public bool? IsValidOnly { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
