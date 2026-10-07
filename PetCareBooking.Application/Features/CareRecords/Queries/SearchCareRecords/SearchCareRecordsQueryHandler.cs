using MediatR;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.CareRecord;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.CareRecords.Queries.SearchCareRecords
{
    public class SearchCareRecordsQueryHandler : IRequestHandler<SearchCareRecordsQuery, PagedResult<CareRecordListResponseDTO>>
    {
        private readonly IGenericRepository<CareRecord> _careRecordRepository;

        public SearchCareRecordsQueryHandler(IGenericRepository<CareRecord> careRecordRepository)
        {
            _careRecordRepository = careRecordRepository;
        }

        public async Task<PagedResult<CareRecordListResponseDTO>> Handle(SearchCareRecordsQuery request, CancellationToken cancellationToken)
        {
            int pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
            int pageSize = request.PageSize < 1 ? 10 : (request.PageSize > 100 ? 100 : request.PageSize);

            var query = _careRecordRepository.GetQueryable()
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                string keyword = request.Keyword.Trim().ToLower();
                query = query.Where(c => (c.Note != null && c.Note.ToLower().Contains(keyword)) ||
                                         (c.HealthStatus != null && c.HealthStatus.ToLower().Contains(keyword)));
            }

            if (request.BookingItemId.HasValue)
            {
                query = query.Where(c => c.BookingItemId == request.BookingItemId.Value);
            }

            if (request.StaffId.HasValue)
            {
                query = query.Where(c => c.StaffId == request.StaffId.Value);
            }

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
