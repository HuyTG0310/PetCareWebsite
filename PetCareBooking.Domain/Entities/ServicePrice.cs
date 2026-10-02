using PetCareBooking.Domain.Common;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Domain.Entities
{
    public class ServicePrice : BaseEntity
    {
        public Guid ServiceId { get; set; }
        public decimal? MinWeight { get; set; }
        public decimal? MaxWeight { get; set; }
        public decimal Price { get; set; }
        public PricingUnit PricingUnit { get; set; }
        public virtual Service Service { get; set; } = null!;
    }
}

