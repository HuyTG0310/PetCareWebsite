using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.Vouchers.Commands.DeleteVoucher
{
    public class DeleteVoucherCommandHandler : IRequestHandler<DeleteVoucherCommand, ApiResponse<bool>>
    {
        private readonly IGenericRepository<Voucher> _voucherRepository;
        private readonly IGenericRepository<Booking> _bookingRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteVoucherCommandHandler(
            IGenericRepository<Voucher> voucherRepository,
            IGenericRepository<Booking> bookingRepository,
            IUnitOfWork unitOfWork)
        {
            _voucherRepository = voucherRepository;
            _bookingRepository = bookingRepository;
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

            // Unlink any bookings referencing this voucher to prevent foreign key constraint violations
            var referencingBookings = await _bookingRepository.GetQueryable()
                .Where(b => b.VoucherId == voucher.Id)
                .ToListAsync(cancellationToken);

            foreach (var booking in referencingBookings)
            {
                booking.VoucherId = null;
                _bookingRepository.Update(booking);
            }

            _voucherRepository.Delete(voucher);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<bool>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Voucher has been deleted successfully.",
                Result = true
            };
        }
    }
}
