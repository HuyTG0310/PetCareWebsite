using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Payment;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.Features.Bookings.Commands.DepositBooking
{
    public class DepositBookingCommandHandler : IRequestHandler<DepositBookingCommand, ApiResponse<PaymentTransactionResponseDTO>>
    {
        private readonly IGenericRepository<Booking> _bookingRepository;
        private readonly IGenericRepository<PaymentTransaction> _paymentTransactionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DepositBookingCommandHandler(
            IGenericRepository<Booking> bookingRepository,
            IGenericRepository<PaymentTransaction> paymentTransactionRepository,
            IUnitOfWork unitOfWork)
        {
            _bookingRepository = bookingRepository;
            _paymentTransactionRepository = paymentTransactionRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<PaymentTransactionResponseDTO>> Handle(DepositBookingCommand request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking == null)
            {
                return new ApiResponse<PaymentTransactionResponseDTO>
                {
                    IsSuccess = false,
                    StatusCode = 404,
                    Message = $"Booking with ID {request.BookingId} not found.",
                    Result = null
                };
            }

            if (booking.Status == BookingStatus.Cancelled)
            {
                return new ApiResponse<PaymentTransactionResponseDTO>
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = "Cannot add deposit to a cancelled booking.",
                    Result = null
                };
            }

            var now = DateTime.UtcNow;
            var transaction = new PaymentTransaction
            {
                Id = Guid.NewGuid(),
                BookingId = booking.Id,
                TransactionType = "DEPOSIT",
                Amount = request.Amount,
                PaymentMethod = request.PaymentMethod.Trim().ToUpper(),
                PaymentStatus = "SUCCESS",
                TransactionCode = !string.IsNullOrWhiteSpace(request.TransactionCode)
                    ? request.TransactionCode.Trim()
                    : $"DEP-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}",
                PaidAt = now,
                Note = request.Note?.Trim() ?? "Deposit payment received",
                CreatedAt = now
            };

            await _paymentTransactionRepository.AddAsync(transaction);

            // If booking items were Pending, confirm them upon receiving deposit
            var bookings = await _bookingRepository.FindAsync(b => b.Id == booking.Id, "BookingItems");
            var bookingWithItems = bookings.FirstOrDefault();
            if (bookingWithItems != null)
            {
                foreach (var item in bookingWithItems.BookingItems)
                {
                    if (item.Status == BookingItemStatus.Pending)
                    {
                        item.Status = BookingItemStatus.Confirmed;
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var responseDto = new PaymentTransactionResponseDTO
            {
                Id = transaction.Id,
                BookingId = transaction.BookingId,
                TransactionType = transaction.TransactionType,
                Amount = transaction.Amount,
                PaymentMethod = transaction.PaymentMethod,
                PaymentStatus = transaction.PaymentStatus,
                TransactionCode = transaction.TransactionCode,
                PaidAt = transaction.PaidAt,
                Note = transaction.Note,
                CreatedAt = transaction.CreatedAt
            };

            return new ApiResponse<PaymentTransactionResponseDTO>
            {
                IsSuccess = true,
                StatusCode = 201,
                Message = $"Deposit of {transaction.Amount:N0} VND recorded successfully.",
                Result = responseDto
            };
        }
    }
}
