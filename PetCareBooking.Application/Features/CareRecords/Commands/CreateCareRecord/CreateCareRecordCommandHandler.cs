using MediatR;
using Microsoft.AspNetCore.Http;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.CareRecords.Commands.CreateCareRecord
{
    public class CreateCareRecordCommandHandler : IRequestHandler<CreateCareRecordCommand, ApiResponse<Guid>>
    {
        private readonly IGenericRepository<CareRecord> _careRecordRepository;
        private readonly IGenericRepository<BookingItem> _bookingItemRepository;
        private readonly IGenericRepository<Staff> _staffRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateCareRecordCommandHandler(
            IGenericRepository<CareRecord> careRecordRepository,
            IGenericRepository<BookingItem> bookingItemRepository,
            IGenericRepository<Staff> staffRepository,
            IUnitOfWork unitOfWork)
        {
            _careRecordRepository = careRecordRepository;
            _bookingItemRepository = bookingItemRepository;
            _staffRepository = staffRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<Guid>> Handle(CreateCareRecordCommand request, CancellationToken cancellationToken)
        {
            // Kiểm tra BookingItem có tồn tại không
            var bookingItem = await _bookingItemRepository.GetByIdAsync(request.BookingItemId);
            if (bookingItem == null)
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Booking item not found.",
                    Result = Guid.Empty
                };
            }

            // Kiểm tra Staff có tồn tại không
            var staff = await _staffRepository.GetByIdAsync(request.StaffId);
            if (staff == null)
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Staff not found.",
                    Result = Guid.Empty
                };
            }

            var careRecord = new CareRecord
            {
                Id = Guid.NewGuid(),
                BookingItemId = request.BookingItemId,
                StaffId = request.StaffId,
                RecordDate = request.RecordDate,
                HealthStatus = request.HealthStatus?.Trim(),
                Note = request.Note?.Trim(),
                ImageUrl = request.ImageUrl?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _careRecordRepository.AddAsync(careRecord);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<Guid>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status201Created,
                Message = "Care record created successfully.",
                Result = careRecord.Id
            };
        }
    }
}
