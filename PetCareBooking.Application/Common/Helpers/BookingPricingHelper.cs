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

            // Only calculate surcharges for uncompleted bookings (ongoing or awaiting check-out)
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

                    // 1. Grace period: Late <= 60 minutes: Free of surcharge
                    if (lateSpan.TotalMinutes <= GracePeriodMinutes)
                    {
                        var graceReason = $"Đón trễ {Math.Ceiling(lateSpan.TotalMinutes)} phút (trong thời gian đệm 60 phút miễn phí)";
                        result.ItemFees[item.Id] = (0, graceReason);
                        reasons.Add($"{item.Pet?.Name ?? "Thú cưng"}: {graceReason}");
                    }
                    // 2. Late between > 1h and <= 6h: Surcharge of 50% daily room price
                    else if (lateHours <= 6)
                    {
                        var itemFee = Math.Round(item.UnitPrice * 0.5m, 0);
                        result.TotalLateFee += itemFee;
                        var reason = $"Đón trễ {Math.Round(lateHours, 1)}h (sau giờ quy định): Phụ phí 50% giá phòng (+{itemFee:N0}đ)";
                        result.ItemFees[item.Id] = (itemFee, reason);
                        reasons.Add($"{item.Pet?.Name ?? "Thú cưng"}: {reason}");
                    }
                    // 3. Late > 6h (after 18:00 or into following days): Surcharge of 100% daily room price per delayed day
                    else
                    {
                        var extraDays = (int)Math.Ceiling(lateHours / 24.0);
                        if (extraDays < 1) extraDays = 1;
                        var itemFee = item.UnitPrice * extraDays;
                        result.TotalLateFee += itemFee;
                        var reason = $"Đón trễ {Math.Round(lateHours, 1)}h (quá 6h/qua đêm): Phụ phí {extraDays} ngày lưu trú (+{itemFee:N0}đ)";
                        result.ItemFees[item.Id] = (itemFee, reason);
                        reasons.Add($"{item.Pet?.Name ?? "Thú cưng"}: {reason}");
                    }
                }
            }

            result.MaxLateHours = Math.Round(result.MaxLateHours, 1);
            result.LateFeeReason = reasons.Any() ? string.Join("; ", reasons) : null;
            return result;
        }
    }
}

