using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Domain.Enums;
using System.Text.Json.Serialization;

namespace PetCareBooking.Application.Features.Bookings.Commands.UpdateBookingStatus
{
    public class UpdateBookingStatusCommand : IRequest<ApiResponse<Guid>>
    {
        [JsonIgnore]
        public Guid Id { get; set; }

        public BookingStatus Status { get; set; }
    }
}