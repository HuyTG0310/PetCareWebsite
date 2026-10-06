using MediatR;
using PetCareBooking.Application.Common.Models;

namespace PetCareBooking.Application.Features.Rooms.Commands.DeleteRoom
{
    public class DeleteRoomCommand : IRequest<ApiResponse<Guid>>
    {
        public Guid Id { get; set; }
    }
}
