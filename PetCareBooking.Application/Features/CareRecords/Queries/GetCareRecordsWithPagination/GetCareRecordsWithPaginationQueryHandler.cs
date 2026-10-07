using MediatR;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.CareRecord;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.CareRecords.Queries.GetCareRecordsWithPagination
{
    public class GetCareRecordsWithPaginationQueryHandler : IRequestHandler<GetCareRecordsWithPaginationQuery, PagedResult<CareRecordListResponseDTO>>
    {
        private readonly IGenericRepository<CareRecord> _careRecordRepository;

        public GetCareRecordsWithPaginationQueryHandler(IGenericRepository<CareRecord> careRecordRepository)
        {
            _careRecordRepository = careRecordRepository;
        }

        public async Task<PagedResult<CareRecordListResponseDTO>> Handle(GetCareRecordsWithPaginationQuery request, CancellationToken cancellationToken)
        {
            int pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
            int pageSize = request.PageSize < 1 ? 10 : (request.PageSize > 100 ? 100 : request.PageSize);

            var query = _careRecordRepository.GetQueryable()
                .AsNoTracking();

            int totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(c => c.RecordDate)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CareRecordListResponseDTO
                {
                    Id = c.Id,
                    BookingItemId = c.BookingItemId,
                    StaffId = c.StaffId,
                    StaffName = c.Staff != null ? c.Staff.FullName : null,
                    RecordDate = c.RecordDate,
                    HealthStatus = c.HealthStatus,
                    ImageUrl = c.ImageUrl
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<CareRecordListResponseDTO>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageIndex,
                PageSize = pageSize
            };
        }
    }
}
