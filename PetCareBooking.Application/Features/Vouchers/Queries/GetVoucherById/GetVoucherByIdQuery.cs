using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Voucher;

namespace PetCareBooking.Application.Features.Vouchers.Queries.GetVoucherById
{
    public class GetVoucherByIdQuery : IRequest<ApiResponse<VoucherResponseDTO>>
    {
        public Guid Id { get; set; }

        public GetVoucherByIdQuery()
        {
        }

        public GetVoucherByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}
