using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Payment;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.Bookings.Queries.GetBookingPayments
{
    public class GetBookingPaymentsQueryHandler : IRequestHandler<GetBookingPaymentsQuery, ApiResponse<List<PaymentTransactionResponseDTO>>>
    {
        private readonly IGenericRepository<Booking> _bookingRepository;
        private readonly IGenericRepository<PaymentTransaction> _paymentTransactionRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetBookingPaymentsQueryHandler(
            IGenericRepository<Booking> bookingRepository,
            IGenericRepository<PaymentTransaction> paymentTransactionRepository,
            ICurrentUserService currentUserService)
        {
            _bookingRepository = bookingRepository;
            _paymentTransactionRepository = paymentTransactionRepository;
            _currentUserService = currentUserService;
        }

        public async Task<ApiResponse<List<PaymentTransactionResponseDTO>>> Handle(GetBookingPaymentsQuery request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking == null)
            {
                return new ApiResponse<List<PaymentTransactionResponseDTO>>
                {
                    IsSuccess = false,
                    StatusCode = 404,
                    Message = $"Booking with ID {request.BookingId} not found.",
                    Result = null
                };
            }

            // Customer IDOR check
            if (!_currentUserService.IsAdminOrStaff)
            {
                if (!_currentUserService.UserId.HasValue || booking.CustomerId != _currentUserService.UserId.Value)
                {
                    return new ApiResponse<List<PaymentTransactionResponseDTO>>
                    {
                        IsSuccess = false,
                        StatusCode = 403,
                        Message = "You do not have permission to view payments for this booking.",
                        Result = null
                    };
                }
            }

            var transactions = await _paymentTransactionRepository.FindAsync(
                t => t.BookingId == booking.Id
            );

            var list = transactions
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new PaymentTransactionResponseDTO
                {
                    Id = t.Id,
                    BookingId = t.BookingId,
                    TransactionType = t.TransactionType,
                    Amount = t.Amount,
                    PaymentMethod = t.PaymentMethod,
                    PaymentStatus = t.PaymentStatus,
                    TransactionCode = t.TransactionCode,
                    PaidAt = t.PaidAt,
                    Note = t.Note,
                    CreatedAt = t.CreatedAt
                }).ToList();

            return new ApiResponse<List<PaymentTransactionResponseDTO>>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = $"Retrieved {list.Count} payment transaction(s).",
                Result = list
            };
        }
    }
}

