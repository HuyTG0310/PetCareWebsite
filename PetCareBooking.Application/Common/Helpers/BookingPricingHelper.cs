using PetCareBooking.Domain.Entities;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.Common.Helpers
{
    public class LateFeeCalculationResult
    {
        public decimal TotalLateFee { get; set; }
        public double MaxLateHours { get; set; }
        public string? LateFeeReason { get; set; }
        public Dictionary<Guid, (decimal Fee, string Reason)> ItemFees { get; set; } = new();
    }

    public static class BookingPricingHelper
    {
        public const int GracePeriodMinutes = 60;

        public static LateFeeCalculationResult CalculateLateFees(Booking booking, DateTime nowUtc)
        {
            var result = new LateFeeCalculationResult();

            // Chỉ tính phụ phí cho đơn chưa hoàn tất (đang diễn ra hoặc đang chờ check-out)
            if (booking.Status == BookingStatus.Completed ||
                booking.Status == BookingStatus.Cancelled ||
                booking.Status == BookingStatus.No_Show)
            {
                return result;
            }

            var boardingItems = booking.BookingItems
                .Where(bi => bi.ScheduledEndAt.HasValue &&
                             bi.Status != BookingItemStatus.Cancelled &&
                             bi.Status != BookingItemStatus.No_Show &&
                             (bi.Service?.ServiceType == ServiceType.Boarding || bi.RoomId.HasValue))
                .ToList();

            var reasons = new List<string>();

            foreach (var item in boardingItems)
            {
                var scheduledEndAt = item.ScheduledEndAt!.Value;
                if (nowUtc > scheduledEndAt)
                {
                    var lateSpan = nowUtc - scheduledEndAt;
                    var lateHours = lateSpan.TotalHours;
                    if (lateHours > result.MaxLateHours)
                    {
                        result.MaxLateHours = lateHours;
                    }

                    // 1. Late <= 60 minutes: Free
                    if (lateSpan.TotalMinutes <= GracePeriodMinutes)
                    {
                        var graceReason = $"Late checkout {Math.Ceiling(lateSpan.TotalMinutes)} minute(s) (within 60 minutes for free)";
                        result.ItemFees[item.Id] = (0, graceReason);
                        reasons.Add($"{item.Pet?.Name ?? "Pet"}: {graceReason}");
                    }

                    // 2. Late from > 1h to <= 6h: Take 50% price
                    else if (lateHours <= 6)
                    {
                        var itemFee = Math.Round(item.UnitPrice * 0.5m, 0);
                        result.TotalLateFee += itemFee;
                        var reason = $"Late checkout {Math.Round(lateHours, 1)}h (regulated hour): Take 50% price (+{itemFee:N0}đ)";
                        result.ItemFees[item.Id] = (itemFee, reason);
                        reasons.Add($"{item.Pet?.Name ?? "Pet"}: {reason}");
                    }
                    // 3. Late > 6h (after 18h): Surcharge 100% price
                    else
                    {
                        var extraDays = (int)Math.Ceiling(lateHours / 24.0);
                        if (extraDays < 1) extraDays = 1;
                        var itemFee = item.UnitPrice * extraDays;
                        result.TotalLateFee += itemFee;
                        var reason = $"Late checkout {Math.Round(lateHours, 1)}h (over 6h/overnight): Surcharge {extraDays} boarding day (+{itemFee:N0}đ)";
                        result.ItemFees[item.Id] = (itemFee, reason);
                        reasons.Add($"{item.Pet?.Name ?? "Pet"}: {reason}");
                    }
                }
            }

            result.MaxLateHours = Math.Round(result.MaxLateHours, 1);
            result.LateFeeReason = reasons.Any() ? string.Join("; ", reasons) : null;
            return result;
        }
    }
}

