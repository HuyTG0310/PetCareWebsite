using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Domain.Enums;
using System.Text.Json.Serialization;

namespace PetCareBooking.Application.Features.Vouchers.Commands.UpdateVoucher
{
    public class UpdateVoucherCommand : IRequest<ApiResponse<Guid>>
    {
        [JsonIgnore]
        public Guid Id { get; set; }

        public string Code { get; set; } = string.Empty;
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? MaxUsage { get; set; }
    }
}
