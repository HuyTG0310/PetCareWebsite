using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Service;
using PetCareBooking.Domain.Enums;
using System.Text.Json.Serialization;

namespace PetCareBooking.Application.Features.Services.Commands.UpdateService
{
    public class UpdateServiceCommand : IRequest<ApiResponse<Guid>>
    {
        [JsonIgnore]
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public ServiceType ServiceType { get; set; }
        public bool IsActive { get; set; }
        public List<ServicePriceDTO> Prices { get; set; } = new List<ServicePriceDTO>();
    }
}
