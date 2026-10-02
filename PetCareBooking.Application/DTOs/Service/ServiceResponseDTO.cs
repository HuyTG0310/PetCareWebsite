using PetCareBooking.Domain.Entities;
using PetCareBooking.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetCareBooking.Application.DTOs.Service
{
    public class ServiceResponseDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public ServiceType ServiceType { get; set; }
        public bool IsActive { get; set; }
        public List<ServicePriceResponseDTO> Prices { get; set; } = new List<ServicePriceResponseDTO>();
    }
}
