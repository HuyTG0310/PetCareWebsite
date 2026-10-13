using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Service;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.Features.Services.Commands.CreateService
{
    public class CreateServiceCommand : IRequest<ApiResponse<Guid>>
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public ServiceType ServiceType { get; set; }
        public string? ImageUrl { get; set; }
        public Guid? RoomTypeId { get; set; }
        public List<ServicePriceDTO> Prices { get; set; } = new List<ServicePriceDTO>();
    }
}
