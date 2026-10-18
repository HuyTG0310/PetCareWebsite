using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.DTOs.Dashboard
{
    public class DashboardStatsResponseDTO
    {
        public RevenueStatsDTO Revenue { get; set; } = new();
        public BookingStatsDTO Bookings { get; set; } = new();
        public RoomStatsDTO Rooms { get; set; } = new();
        public TodayOperationsDTO Operations { get; set; } = new();
        public CustomerStatsDTO Customers { get; set; } = new();
        public List<DailyRevenueTrendDTO> RevenueTrend { get; set; } = new();
        public List<RecentBookingItemDTO> RecentBookings { get; set; } = new();
    }

    public class RevenueStatsDTO
    {
        public decimal TodayRevenue { get; set; }
        public decimal ThisMonthRevenue { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class BookingStatsDTO
    {
        public int TotalBookings { get; set; }
        public int TodayBookings { get; set; }
        public int PendingBookings { get; set; }
        public int ConfirmedBookings { get; set; }
        public int CompletedBookings { get; set; }
        public int CancelledBookings { get; set; }
        public int NoShowBookings { get; set; }
    }

    public class RoomStatsDTO
    {
        public int TotalRooms { get; set; }
        public int OccupiedRooms { get; set; }
        public int AvailableRooms { get; set; }
        public int MaintenanceRooms { get; set; }
        public double OccupancyRate { get; set; }
    }

    public class TodayOperationsDTO
    {
        public int TodayCheckIns { get; set; }
        public int TodayCheckOuts { get; set; }
        public int TodayGroomingPending { get; set; }
        public int TodayGroomingInProgress { get; set; }
        public int TodayGroomingCompleted { get; set; }
        public int ActiveBoardingGuests { get; set; }
    }

    public class CustomerStatsDTO
    {
        public int TotalCustomers { get; set; }
        public int TotalPets { get; set; }
    }

    public class DailyRevenueTrendDTO
    {
        public string Date { get; set; } = string.Empty;
        public string DateLabel { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int BookingCount { get; set; }
    }

    public class RecentBookingItemDTO
    {
        public Guid Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public List<string> PetNames { get; set; } = new();
        public decimal TotalPrice { get; set; }
        public BookingStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public DateTime CreatedAt { get; set; }
        public int ItemsCount { get; set; }
    }
}

