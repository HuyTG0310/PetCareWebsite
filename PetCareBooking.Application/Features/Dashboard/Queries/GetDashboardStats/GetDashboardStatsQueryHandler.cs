using MediatR;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Dashboard;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.Features.Dashboard.Queries.GetDashboardStats
{
    public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, ApiResponse<DashboardStatsResponseDTO>>
    {
        private readonly IGenericRepository<Booking> _bookingRepository;
        private readonly IGenericRepository<BookingItem> _bookingItemRepository;
        private readonly IGenericRepository<PaymentTransaction> _paymentTransactionRepository;
        private readonly IGenericRepository<Room> _roomRepository;
        private readonly IGenericRepository<Customer> _customerRepository;
        private readonly IGenericRepository<Pet> _petRepository;

        public GetDashboardStatsQueryHandler(
            IGenericRepository<Booking> bookingRepository,
            IGenericRepository<BookingItem> bookingItemRepository,
            IGenericRepository<PaymentTransaction> paymentTransactionRepository,
            IGenericRepository<Room> roomRepository,
            IGenericRepository<Customer> customerRepository,
            IGenericRepository<Pet> petRepository)
        {
            _bookingRepository = bookingRepository;
            _bookingItemRepository = bookingItemRepository;
            _paymentTransactionRepository = paymentTransactionRepository;
            _roomRepository = roomRepository;
            _customerRepository = customerRepository;
            _petRepository = petRepository;
        }

        public async Task<ApiResponse<DashboardStatsResponseDTO>> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var todayStart = DateTime.UtcNow.Date;
            var todayEnd = todayStart.AddDays(1);
            var monthStart = new DateTime(todayStart.Year, todayStart.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var nextMonthStart = monthStart.AddMonths(1);

            // 1. REVENUE METRICS
            var successfulPayments = _paymentTransactionRepository.GetQueryable()
                .Where(t => t.PaymentStatus == "SUCCESS");

            var todayRevenue = await successfulPayments
                .Where(t => t.PaidAt >= todayStart && t.PaidAt < todayEnd)
                .SumAsync(t => (decimal?)t.Amount, cancellationToken) ?? 0m;

            var thisMonthRevenue = await successfulPayments
                .Where(t => t.PaidAt >= monthStart && t.PaidAt < nextMonthStart)
                .SumAsync(t => (decimal?)t.Amount, cancellationToken) ?? 0m;

            var totalRevenue = await successfulPayments
                .SumAsync(t => (decimal?)t.Amount, cancellationToken) ?? 0m;

            // 2. BOOKING METRICS
            var bookingsQuery = _bookingRepository.GetQueryable();
            var totalBookings = await bookingsQuery.CountAsync(cancellationToken);
            var todayBookings = await bookingsQuery.CountAsync(b => b.CreatedAt >= todayStart && b.CreatedAt < todayEnd, cancellationToken);
            var pendingBookings = await bookingsQuery.CountAsync(b => b.Status == BookingStatus.Pending, cancellationToken);
            var confirmedBookings = await bookingsQuery.CountAsync(b => b.Status == BookingStatus.Confirmed, cancellationToken);
            var completedBookings = await bookingsQuery.CountAsync(b => b.Status == BookingStatus.Completed, cancellationToken);
            var cancelledBookings = await bookingsQuery.CountAsync(b => b.Status == BookingStatus.Cancelled, cancellationToken);

            // 3. ROOM METRICS
            var roomsQuery = _roomRepository.GetQueryable();
            var totalRooms = await roomsQuery.CountAsync(cancellationToken);
            var maintenanceRooms = await roomsQuery.CountAsync(r => r.Status == RoomStatus.Maintenance, cancellationToken);

            var activeBoardingItems = _bookingItemRepository.GetQueryable()
                .Where(bi => bi.RoomId.HasValue &&
                             (bi.Status == BookingItemStatus.In_Progress || bi.Status == BookingItemStatus.Checked_In || bi.Status == BookingItemStatus.Confirmed) &&
                             bi.ScheduledStartAt <= now && bi.ScheduledEndAt >= now);

            var occupiedRoomIds = await activeBoardingItems
                .Select(bi => bi.RoomId!.Value)
                .Distinct()
                .ToListAsync(cancellationToken);

            var occupiedRoomsCount = occupiedRoomIds.Count;
            var availableRooms = Math.Max(0, totalRooms - maintenanceRooms - occupiedRoomsCount);
            var occupancyRate = totalRooms > 0 ? Math.Round((double)occupiedRoomsCount / totalRooms * 100, 1) : 0;

            // 4. TODAY OPERATIONS
            var bookingItemsQuery = _bookingItemRepository.GetQueryable();

            var todayCheckIns = await bookingItemsQuery.CountAsync(bi =>
                bi.ScheduledStartAt >= todayStart && bi.ScheduledStartAt < todayEnd &&
                bi.Status != BookingItemStatus.Cancelled && bi.Status != BookingItemStatus.No_Show,
                cancellationToken);

            var todayCheckOuts = await bookingItemsQuery.CountAsync(bi =>
                bi.RoomId.HasValue &&
                bi.ScheduledEndAt >= todayStart && bi.ScheduledEndAt < todayEnd &&
                bi.Status != BookingItemStatus.Cancelled && bi.Status != BookingItemStatus.No_Show,
                cancellationToken);

            var todayGroomingPending = await bookingItemsQuery.CountAsync(bi =>
                !bi.RoomId.HasValue &&
                bi.ScheduledStartAt >= todayStart && bi.ScheduledStartAt < todayEnd &&
                (bi.Status == BookingItemStatus.Pending || bi.Status == BookingItemStatus.Confirmed),
                cancellationToken);

            var todayGroomingInProgress = await bookingItemsQuery.CountAsync(bi =>
                !bi.RoomId.HasValue &&
                bi.ScheduledStartAt >= todayStart && bi.ScheduledStartAt < todayEnd &&
                (bi.Status == BookingItemStatus.In_Progress || bi.Status == BookingItemStatus.Checked_In),
                cancellationToken);

            var todayGroomingCompleted = await bookingItemsQuery.CountAsync(bi =>
                !bi.RoomId.HasValue &&
                bi.ScheduledStartAt >= todayStart && bi.ScheduledStartAt < todayEnd &&
                bi.Status == BookingItemStatus.Completed,
                cancellationToken);

            var activeBoardingGuests = await bookingItemsQuery.CountAsync(bi =>
                bi.RoomId.HasValue &&
                (bi.Status == BookingItemStatus.In_Progress ||
                 bi.Status == BookingItemStatus.Checked_In ||
                 (bi.Status == BookingItemStatus.Confirmed && bi.ScheduledStartAt <= now && bi.ScheduledEndAt >= now)),
                cancellationToken);

            // 5. CUSTOMER & PET METRICS
            var totalCustomers = await _customerRepository.GetQueryable().CountAsync(cancellationToken);
            var totalPets = await _petRepository.GetQueryable().CountAsync(p => p.IsActive, cancellationToken);

            // 6. 7-DAY REVENUE & BOOKINGS TREND
            var trendStartDate = todayStart.AddDays(-6);
            var weekTransactions = await successfulPayments
                .Where(t => t.PaidAt >= trendStartDate && t.PaidAt < todayEnd)
                .Select(t => new { t.PaidAt, t.Amount })
                .ToListAsync(cancellationToken);

            var weekBookings = await bookingsQuery
                .Where(b => b.CreatedAt >= trendStartDate && b.CreatedAt < todayEnd)
                .Select(b => new { b.CreatedAt })
                .ToListAsync(cancellationToken);

            var revenueTrend = new List<DailyRevenueTrendDTO>();
            for (int i = 6; i >= 0; i--)
            {
                var day = todayStart.AddDays(-i);
                var nextDay = day.AddDays(1);

                var dayRevenue = weekTransactions
                    .Where(t => t.PaidAt >= day && t.PaidAt < nextDay)
                    .Sum(t => t.Amount);

                var dayBookingsCount = weekBookings
                    .Count(b => b.CreatedAt >= day && b.CreatedAt < nextDay);

                revenueTrend.Add(new DailyRevenueTrendDTO
                {
                    Date = day.ToString("yyyy-MM-dd"),
                    DateLabel = day.ToString("dd/MM"),
                    Revenue = dayRevenue,
                    BookingCount = dayBookingsCount
                });
            }

            // 7. RECENT 5 BOOKINGS
            var recentBookingsRaw = await bookingsQuery
                .Include(b => b.Customer)
                .Include(b => b.BookingItems)
                    .ThenInclude(bi => bi.Pet)
                .OrderByDescending(b => b.CreatedAt)
                .Take(5)
                .ToListAsync(cancellationToken);

            var recentBookings = recentBookingsRaw.Select(b => new RecentBookingItemDTO
            {
                Id = b.Id,
                CustomerName = b.Customer?.FullName ?? "Unknown",
                CustomerPhone = b.Customer?.PhoneNumber ?? "",
                PetNames = b.BookingItems
                    .Where(bi => bi.Pet != null)
                    .Select(bi => bi.Pet!.Name)
                    .Distinct()
                    .ToList(),
                TotalPrice = b.TotalPrice,
                Status = b.Status,
                CreatedAt = b.CreatedAt,
                ItemsCount = b.BookingItems.Count
            }).ToList();

            var responseDto = new DashboardStatsResponseDTO
            {
                Revenue = new RevenueStatsDTO
                {
                    TodayRevenue = todayRevenue,
                    ThisMonthRevenue = thisMonthRevenue,
                    TotalRevenue = totalRevenue
                },
                Bookings = new BookingStatsDTO
                {
                    TotalBookings = totalBookings,
                    TodayBookings = todayBookings,
                    PendingBookings = pendingBookings,
                    ConfirmedBookings = confirmedBookings,
                    CompletedBookings = completedBookings,
                    CancelledBookings = cancelledBookings
                },
                Rooms = new RoomStatsDTO
                {
                    TotalRooms = totalRooms,
                    OccupiedRooms = occupiedRoomsCount,
                    AvailableRooms = availableRooms,
                    MaintenanceRooms = maintenanceRooms,
                    OccupancyRate = occupancyRate
                },
                Operations = new TodayOperationsDTO
                {
                    TodayCheckIns = todayCheckIns,
                    TodayCheckOuts = todayCheckOuts,
                    TodayGroomingPending = todayGroomingPending,
                    TodayGroomingInProgress = todayGroomingInProgress,
                    TodayGroomingCompleted = todayGroomingCompleted,
                    ActiveBoardingGuests = activeBoardingGuests
                },
                Customers = new CustomerStatsDTO
                {
                    TotalCustomers = totalCustomers,
                    TotalPets = totalPets
                },
                RevenueTrend = revenueTrend,
                RecentBookings = recentBookings
            };

            return new ApiResponse<DashboardStatsResponseDTO>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "Get dashboard statistics successfully.",
                Result = responseDto
            };
        }
    }
}

