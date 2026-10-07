using PetCareBooking.Domain.Common;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Domain.Entities
{
    public class Voucher : BaseEntity
    {
        public string Code { get; set; } = null!;
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? MaxUsage { get; set; }
        public int CurrentUsage { get; set; }
    }
}

