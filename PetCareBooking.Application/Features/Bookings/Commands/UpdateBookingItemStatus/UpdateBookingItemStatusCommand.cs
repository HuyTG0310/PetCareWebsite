using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Domain.Enums;
using System.Text.Json.Serialization;

namespace PetCareBooking.Application.Features.Bookings.Commands.UpdateBookingItemStatus
{
    public class UpdateBookingItemStatusCommand : IRequest<ApiResponse<Guid>>
    {
        [JsonIgnore]
        public Guid BookingId { get; set; }

        [JsonIgnore]
        public Guid ItemId { get; set; }

        public BookingItemStatus Status { get; set; }
        public string? ResultNote { get; set; }
        public string? ResultImageUrl { get; set; }
        public Guid? StaffId { get; set; }
        public Guid? RoomId { get; set; }
    }
}