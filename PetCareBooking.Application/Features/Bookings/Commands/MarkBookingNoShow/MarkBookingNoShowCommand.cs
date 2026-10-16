using MediatR;
using PetCareBooking.Application.Common.Models;

namespace PetCareBooking.Application.Features.Bookings.Commands.MarkBookingNoShow
{
    public class MarkBookingNoShowCommand : IRequest<ApiResponse<Guid>>
    {
        public Guid Id { get; set; }
        public string? Reason { get; set; }
    }
}

