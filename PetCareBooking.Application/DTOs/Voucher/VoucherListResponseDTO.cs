using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.DTOs.Voucher
{
    public class VoucherListResponseDTO
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? MaxUsage { get; set; }
        public int CurrentUsage { get; set; }
        public bool IsValid { get; set; }
    }
}
