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

        // List of customer's pets
        public List<PetSummaryDTO> Pets { get; set; } = new List<PetSummaryDTO>();

        // List of customer's recent booking history
        public List<BookingListResponseDTO> Bookings { get; set; } = new List<BookingListResponseDTO>();
    }
}