using PetCareBooking.Domain.Common;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Domain.Entities
{
    public class Service : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public ServiceType ServiceType { get; set; }
        public bool IsActive { get; set; }
        public virtual ICollection<ServicePrice> ServicePrices { get; set; } = new List<ServicePrice>();
    }
}
