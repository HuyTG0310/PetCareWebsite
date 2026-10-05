using PetCareBooking.Application.DTOs.Booking;

namespace PetCareBooking.Application.DTOs.Customer
{
    public class CustomerResponseDTO
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? Address { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // Bổ sung danh sách thú cưng của khách hàng
        public List<PetSummaryDTO> Pets { get; set; } = new List<PetSummaryDTO>();

        // Bổ sung lịch sử các đơn đặt lịch gần đây của khách hàng
        public List<BookingListResponseDTO> Bookings { get; set; } = new List<BookingListResponseDTO>();
    }
}