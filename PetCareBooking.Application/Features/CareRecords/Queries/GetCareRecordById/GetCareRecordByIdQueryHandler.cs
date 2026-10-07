using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.CareRecord;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.CareRecords.Queries.GetCareRecordById
{
    public class GetCareRecordByIdQueryHandler : IRequestHandler<GetCareRecordByIdQuery, ApiResponse<CareRecordResponseDTO>>
    {
        private readonly IGenericRepository<CareRecord> _careRecordRepository;

        public GetCareRecordByIdQueryHandler(IGenericRepository<CareRecord> careRecordRepository)
        {
            _careRecordRepository = careRecordRepository;
        }

        public async Task<ApiResponse<CareRecordResponseDTO>> Handle(GetCareRecordByIdQuery request, CancellationToken cancellationToken)
        {
            var record = await _careRecordRepository.GetQueryable()
                .AsNoTracking()
                .Where(c => c.Id == request.Id)
                .Select(c => new CareRecordResponseDTO
                {
                    Id = c.Id,
                    BookingItemId = c.BookingItemId,
                    StaffId = c.StaffId,
                    StaffName = c.Staff != null ? c.Staff.FullName : null,
                    PetName = c.BookingItem != null && c.BookingItem.Pet != null ? c.BookingItem.Pet.Name : null,
                    RecordDate = c.RecordDate,
                    HealthStatus = c.HealthStatus,
                    Note = c.Note,
                    ImageUrl = c.ImageUrl,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (record == null)
            {
                return new ApiResponse<CareRecordResponseDTO>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"Care record with ID {request.Id} not found.",
                    Result = null
                };
            }

            return new ApiResponse<CareRecordResponseDTO>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Get care record detail successfully.",
                Result = record
            };
        }
    }
}
