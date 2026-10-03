using MediatR;
using PetCareBooking.Application.Common.Models;
using System.Text.Json.Serialization;

namespace PetCareBooking.Application.Features.Bookings.Commands.CancelBooking
{
    public class CancelBookingCommand : IRequest<ApiResponse<Guid>>
    {
        [JsonIgnore]
        public Guid Id { get; set; }

        public string? CancellationReason { get; set; }
    }
}