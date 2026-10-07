using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.CareRecord;

namespace PetCareBooking.Application.Features.CareRecords.Queries.GetCareRecordsWithPagination
{
    public class GetCareRecordsWithPaginationQuery : IRequest<PagedResult<CareRecordListResponseDTO>>
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
