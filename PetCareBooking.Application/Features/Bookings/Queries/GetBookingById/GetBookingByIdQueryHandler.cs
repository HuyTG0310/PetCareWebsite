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

        public GetBookingByIdQueryHandler(IGenericRepository<Booking> bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        public async Task<ApiResponse<BookingResponseDTO>> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
        {
            var bookings = await _bookingRepository.FindAsync(
                b => b.Id == request.Id,
                "BookingItems.Pet,BookingItems.Service,Customer,Promotion,BookingItems.Room,BookingItems.Staff");

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
            var discountAmount = booking.Promotion != null
                ? booking.Promotion.DiscountType == Domain.Enums.DiscountType.Percentage
                    ? booking.BookingItems.Sum(bi => bi.AssignedPrice) * (booking.Promotion.DiscountValue / 100)
                    : booking.Promotion.DiscountValue
                : 0;

            return new BookingResponseDTO
            {
                Id = booking.Id,
                CustomerId = booking.CustomerId,
                CustomerName = booking.Customer.FullName,
                CustomerPhone = booking.Customer.PhoneNumber,
                PromotionId = booking.PromotionId,
                PromotionCode = booking.Promotion?.Code,
                DiscountAmount = discountAmount,
                TotalPrice = booking.TotalPrice,
                Status = booking.Status,
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