using MediatR;
using PetCareBooking.Application.Common.Models;

namespace PetCareBooking.Application.Features.CareRecords.Commands.CreateCareRecord
{
    public class CreateCareRecordCommand : IRequest<ApiResponse<Guid>>
    {
        public Guid BookingItemId { get; set; }
        public Guid StaffId { get; set; }
        public DateTime RecordDate { get; set; } = DateTime.UtcNow;
        public string? HealthStatus { get; set; }
        public string? Note { get; set; }
        public string? ImageUrl { get; set; }
    }
}
