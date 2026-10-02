using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.DTOs.Service
{
    public class ServicePriceDTO
    {
        public decimal? MinWeight { get; set; }
        public decimal? MaxWeight { get; set; }
        public decimal Price { get; set; }
        public PricingUnit PricingUnit { get; set; }
    }
}
