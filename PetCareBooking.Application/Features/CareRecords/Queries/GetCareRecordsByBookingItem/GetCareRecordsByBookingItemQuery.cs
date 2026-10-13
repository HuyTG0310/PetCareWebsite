using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.CareRecord;

namespace PetCareBooking.Application.Features.CareRecords.Queries.GetCareRecordsByBookingItem
{
    public class GetCareRecordsByBookingItemQuery : IRequest<ApiResponse<List<CareRecordResponseDTO>>>
    {
        public Guid BookingItemId { get; set; }

        public GetCareRecordsByBookingItemQuery(Guid bookingItemId)
        {
            BookingItemId = bookingItemId;
        }
    }
}

