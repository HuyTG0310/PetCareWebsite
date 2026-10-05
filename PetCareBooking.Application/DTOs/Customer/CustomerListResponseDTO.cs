namespace PetCareBooking.Application.DTOs.Customer
{
    public class CustomerListResponseDTO
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalPets { get; set; }
    }
}
