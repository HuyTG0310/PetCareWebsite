using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Enums;
using StaffEntity = PetCareBooking.Domain.Entities.Staff;

namespace PetCareBooking.Application.Features.Staffs.Commands.ChangeStaffPassword
{
    public class ChangeStaffPasswordCommandHandler : IRequestHandler<ChangeStaffPasswordCommand, ApiResponse<bool>>
    {
        private readonly IGenericRepository<StaffEntity> _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public ChangeStaffPasswordCommandHandler(
            IGenericRepository<StaffEntity> repository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<ApiResponse<bool>> Handle(ChangeStaffPasswordCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (userId == null)
            {
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Unauthorized access.",
                    Result = false
                };
            }

            var staff = await _repository.GetQueryable()
                .FirstOrDefaultAsync(s => s.Id == userId.Value, cancellationToken);

            if (staff == null || staff.Status != StaffStatus.Active)
            {
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Staff account is inactive or not found.",
                    Result = false
                };
            }

            // Verify current password
            var isCurrentPasswordValid = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, staff.PasswordHash);
            if (!isCurrentPasswordValid)
            {
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Current password is incorrect.",
                    Result = false
                };
            }

            // Reject if new password matches old password
            if (BCrypt.Net.BCrypt.Verify(request.NewPassword, staff.PasswordHash))
            {
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "New password cannot be the same as current password.",
                    Result = false
                };
            }

            staff.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            staff.UpdatedAt = DateTime.UtcNow;

            _repository.Update(staff);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<bool>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Password changed successfully.",
                Result = true
            };
        }
    }
}
