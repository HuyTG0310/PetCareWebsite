using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Voucher;

namespace PetCareBooking.Application.Features.Vouchers.Queries.GetVouchersWithPagination
{
    public class GetVouchersWithPaginationQuery : IRequest<PagedResult<VoucherListResponseDTO>>
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
