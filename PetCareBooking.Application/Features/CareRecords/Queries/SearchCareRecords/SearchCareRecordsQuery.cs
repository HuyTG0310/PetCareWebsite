using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.CareRecord;

namespace PetCareBooking.Application.Features.CareRecords.Queries.SearchCareRecords
{
    public class SearchCareRecordsQuery : IRequest<PagedResult<CareRecordListResponseDTO>>
    {
        public string? Keyword { get; set; }
        public Guid? BookingItemId { get; set; }
        public Guid? StaffId { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
