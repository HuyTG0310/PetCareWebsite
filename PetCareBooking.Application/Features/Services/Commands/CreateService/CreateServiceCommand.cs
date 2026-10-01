using MediatR;
using PetCareBooking.Application.Common.Models;

namespace PetCareBooking.Application.Features.Services.Commands.CreateService
{
    public class CreateServiceCommand : IRequest<ApiResponse<Guid>>
    {
        public string ServiceName { get; set; } = null!;
        public string ServiceType { get; set; } = null!;
        public string? Description { get; set; }
        public int? DurationMinutes { get; set; }
        public string PricingUnit { get; set; } = null!;
        public decimal Price { get; set; }
    }
}
