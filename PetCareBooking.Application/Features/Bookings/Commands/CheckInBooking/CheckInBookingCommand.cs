using MediatR;
using PetCareBooking.Application.Common.Models;
using System.Text.Json.Serialization;

namespace PetCareBooking.Application.Features.Bookings.Commands.CheckInBooking
{
    public class CheckInBookingCommand : IRequest<ApiResponse<Guid>>
    {
        [JsonIgnore]
        public Guid Id { get; set; }
    }
}
