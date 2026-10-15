using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.DTOs.Booking
{
    public class BookingReceiptResponseDTO
    {
        // Salon / Business Info
        public StoreInfoDTO Store { get; set; } = new();

        // Receipt Meta Information
        public string ReceiptNo { get; set; } = null!;
        public Guid BookingId { get; set; }
        public DateTime PrintedAt { get; set; }
        public DateTime BookingCreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string BookingStatus { get; set; } = null!;
        public string CashierName { get; set; } = null!;

        // Customer Information
        public ReceiptCustomerDTO Customer { get; set; } = null!;

        // Pets Summary
        public List<ReceiptPetSummaryDTO> Pets { get; set; } = new();

        // Line Items (Chi tiết từng dịch vụ)
        public List<ReceiptItemDTO> Items { get; set; } = new();

        // Financial Breakdown (Bảng tính chi phí & giảm giá)
        public decimal SubTotal { get; set; }
        public Guid? VoucherId { get; set; }
        public string? VoucherCode { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal RemainingBalance { get; set; }
        public string PaymentStatus { get; set; } = null!; // PAID, PARTIALLY_PAID, UNPAID
        public string PrimaryPaymentMethod { get; set; } = null!;

        // Payment Transactions (Lịch sử các đợt thanh toán)
        public List<ReceiptPaymentTransactionDTO> PaymentHistory { get; set; } = new();

        // Receipt Footer & Policies
        public string FooterNote { get; set; } = "Cảm ơn Quý khách và các bé cưng đã sử dụng dịch vụ tại Pet Care! Quý khách vui lòng kiểm tra hóa đơn và tình trạng của bé trước khi rời quầy.";
        public string HotlineSupport { get; set; } = "1900 8888 / 0901 234 567";
    }

    public class StoreInfoDTO
    {
        public string StoreName { get; set; } = "PET CARE & BOARDING SPA";
        public string Address { get; set; } = "123 Đường Số 7, Phường An Phú, TP. Thủ Đức, TP. Hồ Chí Minh";
        public string Phone { get; set; } = "0901 234 567";
        public string Email { get; set; } = "support@petcarebooking.vn";
        public string Website { get; set; } = "https://petcarebooking.vn";
        public string TaxCode { get; set; } = "0312345678";
    }

    public class ReceiptCustomerDTO
    {
        public Guid CustomerId { get; set; }
        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Address { get; set; }
    }

    public class ReceiptPetSummaryDTO
    {
        public Guid PetId { get; set; }
        public string Name { get; set; } = null!;
        public string Species { get; set; } = null!;
        public string? Breed { get; set; }
        public decimal Weight { get; set; }
    }

    public class ReceiptItemDTO
    {
        public int ItemIndex { get; set; }
        public Guid ItemId { get; set; }
        public Guid PetId { get; set; }
        public string PetName { get; set; } = null!;
        public string ServiceName { get; set; } = null!;
        public string ServiceType { get; set; } = null!; // Grooming / Boarding
        public string? RoomInfo { get; set; }
        public string? StaffName { get; set; }
        public DateTime ScheduledStartAt { get; set; }
        public DateTime? ScheduledEndAt { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = null!;
        public string? ResultNote { get; set; }
    }

    public class ReceiptPaymentTransactionDTO
    {
        public Guid TransactionId { get; set; }
        public string? TransactionCode { get; set; }
        public string TransactionType { get; set; } = null!; // PAYMENT, DEPOSIT
        public string PaymentMethod { get; set; } = null!; // CASH, BANK_TRANSFER, VNPAY, etc.
        public decimal Amount { get; set; }
        public DateTime? PaidAt { get; set; }
        public string PaymentStatus { get; set; } = null!;
        public string? Note { get; set; }
    }
}
