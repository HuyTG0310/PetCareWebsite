using MediatR;
using Microsoft.AspNetCore.Http;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Enums;
using StaffEntity = PetCareBooking.Domain.Entities.Staff;

namespace PetCareBooking.Application.Features.Staffs.Commands.DeleteStaff
{
    public class DeleteStaffCommandHandler : IRequestHandler<DeleteStaffCommand, ApiResponse<bool>>
    {
        private readonly IGenericRepository<StaffEntity> _repository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteStaffCommandHandler(
            IGenericRepository<StaffEntity> repository,
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<bool>> Handle(DeleteStaffCommand request, CancellationToken cancellationToken)
        {
            var staff = await _repository.GetByIdAsync(request.Id);
            if (staff == null)
            {
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Staff not found.",
                    Result = false
                };
            }

            if (staff.Status == StaffStatus.Inactive)
            {
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Staff is already inactive.",
                    Result = false
                };
            }

            // Deactivate staff account
            staff.Status = StaffStatus.Inactive;
            staff.UpdatedAt = DateTime.UtcNow;
            _repository.Update(staff);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<bool>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Staff deactivated successfully.",
                Result = true
            };
        }
    }
}