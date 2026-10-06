using MediatR;
using PetCareBooking.Application.Common.Models;

namespace PetCareBooking.Application.Features.RoomTypes.Commands.DeleteRoomType
{
    public class DeleteRoomTypeCommand : IRequest<ApiResponse<Guid>>
    {
        public Guid Id { get; set; }
    }
}
