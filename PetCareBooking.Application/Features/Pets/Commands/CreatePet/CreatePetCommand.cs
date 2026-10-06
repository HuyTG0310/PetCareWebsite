using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.Features.Pets.Commands.CreatePet
{
    public class CreatePetCommand : IRequest<ApiResponse<Guid>>
    {
        public Guid CustomerId { get; set; }
        public string Name { get; set; } = null!;
        public PetSpecies Species { get; set; }
        public string? Breed { get; set; }
        public decimal Weight { get; set; }
        public int? Age { get; set; }
        public string? HealthNotes { get; set; }
    }
}
