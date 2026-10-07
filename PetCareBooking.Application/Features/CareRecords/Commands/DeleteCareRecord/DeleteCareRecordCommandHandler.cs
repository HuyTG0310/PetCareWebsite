using MediatR;
using Microsoft.AspNetCore.Http;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.CareRecords.Commands.DeleteCareRecord
{
    public class DeleteCareRecordCommandHandler : IRequestHandler<DeleteCareRecordCommand, ApiResponse<bool>>
    {
        private readonly IGenericRepository<CareRecord> _careRecordRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteCareRecordCommandHandler(
            IGenericRepository<CareRecord> careRecordRepository,
            IUnitOfWork unitOfWork)
        {
            _careRecordRepository = careRecordRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<bool>> Handle(DeleteCareRecordCommand request, CancellationToken cancellationToken)
        {
            var careRecord = await _careRecordRepository.GetByIdAsync(request.Id);
            if (careRecord == null)
            {
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"Care record with ID {request.Id} not found.",
                    Result = false
                };
            }
            _careRecordRepository.Delete(careRecord);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<bool>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Care record deleted successfully.",
                Result = true
            };
        }
    }
}
