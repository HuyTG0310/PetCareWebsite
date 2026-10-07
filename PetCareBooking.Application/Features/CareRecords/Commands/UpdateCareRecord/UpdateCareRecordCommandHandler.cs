using MediatR;
using Microsoft.AspNetCore.Http;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.CareRecords.Commands.UpdateCareRecord
{
    public class UpdateCareRecordCommandHandler : IRequestHandler<UpdateCareRecordCommand, ApiResponse<Guid>>
    {
        private readonly IGenericRepository<CareRecord> _careRecordRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateCareRecordCommandHandler(
            IGenericRepository<CareRecord> careRecordRepository,
            IUnitOfWork unitOfWork)
        {
            _careRecordRepository = careRecordRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<Guid>> Handle(UpdateCareRecordCommand request, CancellationToken cancellationToken)
        {
            var careRecord = await _careRecordRepository.GetByIdAsync(request.Id);
            if (careRecord == null)
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"Care record with ID {request.Id} not found.",
                    Result = Guid.Empty
                };
            }

            if (request.RecordDate.HasValue)
            {
                careRecord.RecordDate = request.RecordDate.Value;
            }

            careRecord.HealthStatus = request.HealthStatus?.Trim();
            careRecord.Note = request.Note?.Trim();
            careRecord.ImageUrl = request.ImageUrl?.Trim();
            careRecord.UpdatedAt = DateTime.UtcNow;

            _careRecordRepository.Update(careRecord);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<Guid>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Care record updated successfully.",
                Result = careRecord.Id
            };
        }
    }
}
