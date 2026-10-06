using MediatR;
using PetCareBooking.Application.Common.Models;

namespace PetCareBooking.Application.Features.Pets.Commands.DeletePet
{
    public class DeletePetCommand : IRequest<ApiResponse<bool>>
    {
        public Guid Id { get; set; }

        public DeletePetCommand()
        {
        }

        public DeletePetCommand(Guid id)
        {
            Id = id;
        }
    }
}
