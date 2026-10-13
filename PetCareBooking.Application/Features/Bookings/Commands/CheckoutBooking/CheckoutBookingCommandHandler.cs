using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Payment;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.Features.Bookings.Commands.CheckoutBooking
{
    public class CheckoutBookingCommandHandler : IRequestHandler<CheckoutBookingCommand, ApiResponse<CheckoutResponseDTO>>
    {
        private readonly IGenericRepository<Booking> _bookingRepository;
        private readonly IGenericRepository<BookingItem> _bookingItemRepository;
        private readonly IGenericRepository<PaymentTransaction> _paymentTransactionRepository;
        private readonly IGenericRepository<Room> _roomRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CheckoutBookingCommandHandler(
            IGenericRepository<Booking> bookingRepository,
            IGenericRepository<BookingItem> bookingItemRepository,
            IGenericRepository<PaymentTransaction> paymentTransactionRepository,
            IGenericRepository<Room> roomRepository,
            IUnitOfWork unitOfWork)
        {
            _bookingRepository = bookingRepository;
            _bookingItemRepository = bookingItemRepository;
            _paymentTransactionRepository = paymentTransactionRepository;
            _roomRepository = roomRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<CheckoutResponseDTO>> Handle(CheckoutBookingCommand request, CancellationToken cancellationToken)
        {
            // 1. Fetch booking with items and room navigation
            var bookings = await _bookingRepository.FindAsync(
                b => b.Id == request.BookingId,
                "BookingItems.Room"
            );

            var booking = bookings.FirstOrDefault();
            if (booking == null)
            {
                return new ApiResponse<CheckoutResponseDTO>
                {
                    IsSuccess = false,
                    StatusCode = 404,
                    Message = $"Booking with ID {request.BookingId} not found.",
                    Result = null
                };
            }

            // 2. Validate booking status
            if (booking.Status == BookingStatus.Cancelled)
            {
                return new ApiResponse<CheckoutResponseDTO>
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = "Cannot checkout a cancelled booking.",
                    Result = null
                };
            }

            // 3. Fetch previous payment transactions
            var existingTransactions = await _paymentTransactionRepository.FindAsync(
                t => t.BookingId == booking.Id
            );

            var successfulTransactions = existingTransactions
                .Where(t => t.PaymentStatus == "SUCCESS")
                .ToList();

            var totalPaid = successfulTransactions.Sum(t => t.Amount);
            var remainingAmount = Math.Max(0, booking.TotalPrice - totalPaid);

            // If already fully paid and completed
            if (booking.Status == BookingStatus.Completed && remainingAmount <= 0)
            {
                return new ApiResponse<CheckoutResponseDTO>
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = "Booking is already completed and fully settled.",
                    Result = null
                };
            }

            // 4. Calculate amount for this transaction
            var paymentAmount = (request.Amount.HasValue && request.Amount.Value > 0)
                ? request.Amount.Value
                : remainingAmount;

            var transactionMethod = request.PaymentMethod.Trim().ToUpper();
            var transactionCode = !string.IsNullOrWhiteSpace(request.TransactionCode)
                ? request.TransactionCode.Trim()
                : $"TXN-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";

            var now = DateTime.UtcNow;

            // 5. Create PaymentTransaction (only if an additional payment amount is made at counter)
            PaymentTransaction? transaction = null;
            if (paymentAmount > 0)
            {
                transaction = new PaymentTransaction
                {
                    Id = Guid.NewGuid(),
                    BookingId = booking.Id,
                    TransactionType = "PAYMENT",
                    Amount = paymentAmount,
                    PaymentMethod = transactionMethod,
                    PaymentStatus = "SUCCESS",
                    TransactionCode = transactionCode,
                    PaidAt = now,
                    Note = request.Note?.Trim() ?? "Checkout and settled at counter",
                    CreatedAt = now
                };

                await _paymentTransactionRepository.AddAsync(transaction);
            }

            // 6. Update booking status and release rooms if fully settled
            var newTotalPaid = totalPaid + paymentAmount;
            var newRemainingAmount = Math.Max(0, booking.TotalPrice - newTotalPaid);
            var releasedRooms = new List<string>();

            if (newTotalPaid >= booking.TotalPrice)
            {
                booking.Status = BookingStatus.Completed;

                // Mark all active booking items as Completed
                foreach (var item in booking.BookingItems)
                {
                    if (item.Status != BookingItemStatus.Cancelled && item.Status != BookingItemStatus.No_Show)
                    {
                        item.Status = BookingItemStatus.Completed;
                        _bookingItemRepository.Update(item);
                    }
                }

                // Release rooms for boarding items
                var distinctRoomIds = booking.BookingItems
                    .Where(bi => bi.RoomId.HasValue)
                    .Select(bi => bi.RoomId!.Value)
                    .Distinct()
                    .ToList();

                foreach (var roomId in distinctRoomIds)
                {
                    var room = await _roomRepository.GetByIdAsync(roomId);
                    if (room != null)
                    {
                        releasedRooms.Add(room.RoomName);
                    }
                }
            }
            else
            {
                // Partial payment received
                booking.Status = BookingStatus.Confirmed;
            }

            _bookingRepository.Update(booking);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var responseDto = new CheckoutResponseDTO
            {
                BookingId = booking.Id,
                TransactionId = transaction?.Id ?? Guid.Empty,
                TransactionCode = transaction?.TransactionCode ?? "PREPAID_ONLINE",
                PaymentMethod = transaction?.PaymentMethod ?? "PREPAID",
                AmountPaid = paymentAmount,
                TotalPaid = newTotalPaid,
                RemainingAmount = newRemainingAmount,
                BookingStatus = booking.Status,
                PaidAt = now,
                ReleasedRooms = releasedRooms,
                Message = (newTotalPaid >= booking.TotalPrice)
                    ? "Checkout and handover completed successfully. Pet care services closed and room(s) released."
                    : $"Partial payment of {paymentAmount:N0} VND recorded. Remaining balance: {newRemainingAmount:N0} VND."
            };

            return new ApiResponse<CheckoutResponseDTO>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = responseDto.Message,
                Result = responseDto
            };
        }
    }
}
