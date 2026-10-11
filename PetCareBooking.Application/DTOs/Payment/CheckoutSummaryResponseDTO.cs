using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.DTOs.Payment
{
    public class CheckoutSummaryResponseDTO
    {
        public Guid BookingId { get; set; }
        public BookingStatus BookingStatus { get; set; }
        public DateTime BookingCreatedAt { get; set; }

        // Customer Details
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = null!;
        public string CustomerPhone { get; set; } = null!;
        public string CustomerEmail { get; set; } = null!;

        // Items Details
        public List<CheckoutItemSummaryDTO> Items { get; set; } = new List<CheckoutItemSummaryDTO>();

        // Financial Details
        public decimal SubTotal { get; set; }
        public Guid? VoucherId { get; set; }
        public string? VoucherCode { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal RemainingAmount { get; set; }
        public string PaymentStatus { get; set; } = "UNPAID"; // UNPAID, PARTIALLY_PAID, PAID
        public bool CanCheckout { get; set; }

        // Previous Payments
        public List<PaymentTransactionResponseDTO> PaymentHistory { get; set; } = new List<PaymentTransactionResponseDTO>();
    }

    public class CheckoutItemSummaryDTO
    {
        public Guid ItemId { get; set; }
        public Guid PetId { get; set; }
        public string PetName { get; set; } = null!;
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; } = null!;
        public ServiceType ServiceType { get; set; }
        public Guid? RoomId { get; set; }
        public string? RoomName { get; set; }
        public Guid? StaffId { get; set; }
        public string? StaffName { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal AssignedPrice { get; set; }
        public BookingItemStatus Status { get; set; }
        public DateTime ScheduledStartAt { get; set; }
        public DateTime? ScheduledEndAt { get; set; }
    }
}

