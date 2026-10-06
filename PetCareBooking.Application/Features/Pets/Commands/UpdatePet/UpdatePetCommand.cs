using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Domain.Enums;
using System.Text.Json.Serialization;

namespace PetCareBooking.Application.Features.Pets.Commands.UpdatePet
{
    public class UpdatePetCommand : IRequest<ApiResponse<Guid>>
    {
        [JsonIgnore]
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;
        public PetSpecies Species { get; set; }
        public string? Breed { get; set; }
        public decimal Weight { get; set; }
        public int? Age { get; set; }
        public string? HealthNotes { get; set; }
    }
}
