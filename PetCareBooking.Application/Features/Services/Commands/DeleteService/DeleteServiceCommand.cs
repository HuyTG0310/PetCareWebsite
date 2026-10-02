using MediatR;
using PetCareBooking.Application.Common.Models;

namespace PetCareBooking.Application.Features.Services.Commands.DeleteService
{
    public class DeleteServiceCommand : IRequest<ApiResponse<Guid>>
    {
        public Guid Id { get; set; }
    }
}
