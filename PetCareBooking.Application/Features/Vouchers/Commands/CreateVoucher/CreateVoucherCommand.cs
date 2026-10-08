using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.Features.Vouchers.Commands.CreateVoucher
{
    public class CreateVoucherCommand : IRequest<ApiResponse<Guid>>
    {
        public string Code { get; set; } = string.Empty;
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? MaxUsage { get; set; }
    }
}

