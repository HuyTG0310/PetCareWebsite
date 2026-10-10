using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Booking;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.Bookings.Queries.GetBookingById
{
    public class GetBookingByIdQueryHandler : IRequestHandler<GetBookingByIdQuery, ApiResponse<BookingResponseDTO>>
    {
        private readonly IGenericRepository<Booking> _bookingRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetBookingByIdQueryHandler(
            IGenericRepository<Booking> bookingRepository,
            ICurrentUserService currentUserService)
        {
            _bookingRepository = bookingRepository;
            _currentUserService = currentUserService;
        }

        public async Task<ApiResponse<BookingResponseDTO>> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
        {
            var bookings = await _bookingRepository.FindAsync(
                b => b.Id == request.Id,
                "BookingItems.Pet,BookingItems.Service,Customer,Voucher,BookingItems.Room,BookingItems.Staff");

            var booking = bookings.FirstOrDefault();

            if (booking == null)
            {
                return new ApiResponse<BookingResponseDTO>
                {
                    IsSuccess = false,
                    StatusCode = 404,
                    Message = $"Booking with ID {request.Id} not found.",
                    Result = null
                };
            }

            // Phân quyền: Customer chỉ được xem booking của chính mình
            if (!_currentUserService.IsAdminOrStaff)
            {
                if (!_currentUserService.UserId.HasValue || booking.CustomerId != _currentUserService.UserId.Value)
                {
                    return new ApiResponse<BookingResponseDTO>
                    {
                        IsSuccess = false,
                        StatusCode = 403,
                        Message = "You do not have permission to view this booking.",
                        Result = null
                    };
                }
            }

            var bookingDTO = MapToBookingResponseDTO(booking);

            return new ApiResponse<BookingResponseDTO>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "Get booking details successfully.",
                Result = bookingDTO
            };
        }

        private BookingResponseDTO MapToBookingResponseDTO(Booking booking)
        {
            var discountAmount = booking.Voucher != null
                ? booking.Voucher.DiscountType == Domain.Enums.DiscountType.Percentage
                    ? booking.BookingItems.Sum(bi => bi.AssignedPrice) * (booking.Voucher.DiscountValue / 100)
                    : booking.Voucher.DiscountValue
                : 0;

            return new BookingResponseDTO
            {
                Id = booking.Id,
                CustomerId = booking.CustomerId,
                CustomerName = booking.Customer.FullName,
                CustomerPhone = booking.Customer.PhoneNumber,
                VoucherId = booking.VoucherId,
                VoucherCode = booking.Voucher?.Code,
                DiscountAmount = discountAmount,
                TotalPrice = booking.TotalPrice,
                Status = booking.Status,
                CancellationReason = booking.CancellationReason,
                CreatedAt = booking.CreatedAt,
                BookingItems = booking.BookingItems.Select(bi => new BookingItemResponseDTO
                {
                    Id = bi.Id,
                    PetId = bi.PetId,
                    PetName = bi.Pet.Name,
                    PetSpecies = bi.Pet.Species,
                    PetWeight = bi.Pet.Weight,
                    ServiceId = bi.ServiceId,
                    ServiceName = bi.Service.Name,
                    ServiceType = bi.Service.ServiceType,
                    ScheduledStartAt = bi.ScheduledStartAt,
                    ScheduledEndAt = bi.ScheduledEndAt,
                    Quantity = bi.Quantity,
                    UnitPrice = bi.UnitPrice,
                    AssignedPrice = bi.AssignedPrice,
                    Status = bi.Status,
                    RoomId = bi.RoomId,
                    RoomName = bi.Room?.RoomName,
                    StaffId = bi.StaffId,
                    StaffName = bi.Staff?.FullName,
                    ResultNote = bi.ResultNote,
                    ResultImageUrl = bi.ResultImageUrl
                }).ToList()
            };
        }
    }
}
