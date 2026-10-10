using MediatR;
using Microsoft.AspNetCore.Http;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.Vouchers.Commands.DeleteVoucher
{
    public class DeleteVoucherCommandHandler : IRequestHandler<DeleteVoucherCommand, ApiResponse<bool>>
    {
        private readonly IGenericRepository<Voucher> _voucherRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteVoucherCommandHandler(IGenericRepository<Voucher> voucherRepository, IUnitOfWork unitOfWork)
        {
            _voucherRepository = voucherRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<bool>> Handle(DeleteVoucherCommand request, CancellationToken cancellationToken)
        {
            var voucher = await _voucherRepository.GetByIdAsync(request.Id);
            if (voucher == null)
            {
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"Voucher with ID {request.Id} not found.",
                    Result = false
                };
            }
            if (voucher.CurrentUsage == 0)
            {
                _voucherRepository.Delete(voucher);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new ApiResponse<bool>
                {
                    IsSuccess = true,
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Voucher has been permanently deleted successfully.",
                    Result = true
                };
            }
            else
            {
                voucher.EndDate = DateTime.UtcNow;
                voucher.UpdatedAt = DateTime.UtcNow;

                _voucherRepository.Update(voucher);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new ApiResponse<bool>
                {
                    IsSuccess = true,
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Voucher has already been used and cannot be permanently deleted. It has been deactivated by setting its end date to now.",
                    Result = true
                };
            }
        }
    }
}
