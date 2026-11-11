using MediatR;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;
using PetCareBooking.Domain.Enums;

using Microsoft.AspNetCore.Http;
using PetCareBooking.Application.DTOs.Booking;

namespace PetCareBooking.Application.Features.Bookings.Commands.CreateBooking
{
    public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, ApiResponse<CreateBookingResponseDTO>>
    {
        private readonly IGenericRepository<Booking> _bookingRepository;
        private readonly IGenericRepository<Customer> _customerRepository;
        private readonly IGenericRepository<Pet> _petRepository;
        private readonly IGenericRepository<Service> _serviceRepository;
        private readonly IGenericRepository<Voucher> _voucherRepository;
        private readonly IGenericRepository<ServicePrice> _servicePriceRepository;
        private readonly IGenericRepository<Room> _roomRepository;
        private readonly IGenericRepository<BookingItem> _bookingItemRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IVnPayService _vnPayService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;

        public CreateBookingCommandHandler(
            IGenericRepository<Booking> bookingRepository,
            IGenericRepository<Customer> customerRepository,
            IGenericRepository<Pet> petRepository,
            IGenericRepository<Service> serviceRepository,
            IGenericRepository<Voucher> voucherRepository,
            IGenericRepository<ServicePrice> servicePriceRepository,
            IGenericRepository<Room> roomRepository,
            IGenericRepository<BookingItem> bookingItemRepository,
            ICurrentUserService currentUserService,
            IVnPayService vnPayService,
            IHttpContextAccessor httpContextAccessor,
            IUnitOfWork unitOfWork)
        {
            _bookingRepository = bookingRepository;
            _customerRepository = customerRepository;
            _petRepository = petRepository;
            _serviceRepository = serviceRepository;
            _voucherRepository = voucherRepository;
            _servicePriceRepository = servicePriceRepository;
            _roomRepository = roomRepository;
            _bookingItemRepository = bookingItemRepository;
            _currentUserService = currentUserService;
            _vnPayService = vnPayService;
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<CreateBookingResponseDTO>> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
        {
            Guid targetCustomerId;

            if (!_currentUserService.IsAdminOrStaff)
            {
                if (!_currentUserService.UserId.HasValue)
                {
                    return new ApiResponse<CreateBookingResponseDTO>
                    {
                        IsSuccess = false,
                        StatusCode = 401,
                        Message = "User is not authenticated.",
                        Result = null
                    };
                }

                targetCustomerId = _currentUserService.UserId.Value;
            }
            else
            {
                if (!_currentUserService.HasPermission("BOOKING_CREATE"))
                {
                    return new ApiResponse<CreateBookingResponseDTO>
                    {
                        IsSuccess = false,
                        StatusCode = StatusCodes.Status403Forbidden,
                        Message = "You do not have permission to create bookings at the counter (BOOKING_CREATE permission required).",
                        Result = null
                    };
                }

                if (!request.CustomerId.HasValue || request.CustomerId.Value == Guid.Empty)
                {
                    return new ApiResponse<CreateBookingResponseDTO>
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = "CustomerId is required when staff creates a booking on behalf of a customer.",
                        Result = null
                    };
                }

                targetCustomerId = request.CustomerId.Value;
            }

            // 1. Validate customer exists
            var customers = await _customerRepository.FindAsync(c => c.Id == targetCustomerId);
            var customer = customers.FirstOrDefault();

            if (customer == null)
            {
                return new ApiResponse<CreateBookingResponseDTO>
                {
                    IsSuccess = false,
                    StatusCode = 404,
                    Message = "Customer not found.",
                    Result = null
                };
            }

            // 2. Validate all pets exist and belong to customer
            var petIds = request.BookingItems.Select(bi => bi.PetId).Distinct().ToList();
            var pets = await _petRepository.FindAsync(p => petIds.Contains(p.Id));

            foreach (var petId in petIds)
            {
                var pet = pets.FirstOrDefault(p => p.Id == petId);
                if (pet == null)
                {
                    return new ApiResponse<CreateBookingResponseDTO>
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = $"Pet with ID {petId} not found.",
                        Result = null
                    };
                }

                if (pet.CustomerId != targetCustomerId)
                {
                    return new ApiResponse<CreateBookingResponseDTO>
                    {
                        IsSuccess = false,
                        StatusCode = 403,
                        Message = $"Pet with ID {petId} does not belong to this customer.",
                        Result = null
                    };
                }
            }

            // 3. Validate all services exist and are active
            var serviceIds = request.BookingItems.Select(bi => bi.ServiceId).Distinct().ToList();
            var services = await _serviceRepository.FindAsync(s => serviceIds.Contains(s.Id), "ServicePrices", "RoomType");

            foreach (var serviceId in serviceIds)
            {
                var service = services.FirstOrDefault(s => s.Id == serviceId);
                if (service == null)
                {
                    return new ApiResponse<CreateBookingResponseDTO>
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = $"Service with ID {serviceId} not found.",
                        Result = null
                    };
                }

                if (!service.IsActive)
                {
                    return new ApiResponse<CreateBookingResponseDTO>
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = $"Service {service.Name} is not active.",
                        Result = null
                    };
                }
            }

            // Boarding service requires 100% online payment via VNPay to secure room reservation.
            var hasBoardingService = services.Any(s => s.ServiceType == ServiceType.Boarding);
            var paymentChoice = request.PaymentChoice?.Trim().ToUpper() ?? "PAY_AT_COUNTER";

            if (hasBoardingService && !_currentUserService.IsAdminOrStaff && paymentChoice != "VNPAY")
            {
                return new ApiResponse<CreateBookingResponseDTO>
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = "The accommodation booking service requires 100% online payment via VNPay to secure your reservation.",
                    Result = null
                };
            }

            // 4. Validate and apply voucher
            Guid? voucherId = null;
            decimal discountAmount = 0;
            Voucher? appliedVoucher = null;

            if (!string.IsNullOrEmpty(request.VoucherCode))
            {
                var trimmedCode = request.VoucherCode.Trim().ToLower();
                var vouchers = await _voucherRepository.FindAsync(v =>
                    v.Code.ToLower() == trimmedCode &&
                    v.StartDate <= DateTime.UtcNow &&
                    v.EndDate >= DateTime.UtcNow);

                var voucher = vouchers.FirstOrDefault();

                if (voucher == null)
                {
                    return new ApiResponse<CreateBookingResponseDTO>
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = "Invalid or expired voucher code.",
                        Result = null
                    };
                }

                if (voucher.MaxUsage.HasValue && voucher.CurrentUsage >= voucher.MaxUsage.Value)
                {
                    return new ApiResponse<CreateBookingResponseDTO>
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = "Voucher usage limit has been reached.",
                        Result = null
                    };
                }

                voucherId = voucher.Id;
                appliedVoucher = voucher;
            }

            // 5. Create booking with items and calculate prices
            var newBooking = new Booking
            {
                Id = Guid.NewGuid(),
                CustomerId = targetCustomerId,
                VoucherId = voucherId,
                Status = BookingStatus.Pending,
                BookingItems = new List<BookingItem>()
            };

            decimal subtotal = 0;
            var allocatedRoomIdsInThisBooking = new HashSet<Guid>();

            foreach (var itemRequest in request.BookingItems)
            {
                var pet = pets.First(p => p.Id == itemRequest.PetId);
                var service = services.First(s => s.Id == itemRequest.ServiceId);

                // Calculate unit price based on pet weight and service pricing tiers
                var unitPrice = CalculateUnitPrice(service, pet.Weight);

                if (unitPrice == null)
                {
                    return new ApiResponse<CreateBookingResponseDTO>
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = $"No pricing found for {service.Name} with pet weight {pet.Weight}kg.",
                        Result = null
                    };
                }

                var itemStartAt = itemRequest.ScheduledStartAt;
                var itemEndAt = itemRequest.ScheduledEndAt;
                decimal durationDays = 1;
                Guid? assignedRoomId = null;

                if (service.ServiceType == ServiceType.Boarding)
                {
                    if (!itemRequest.ScheduledEndAt.HasValue)
                    {
                        return new ApiResponse<CreateBookingResponseDTO>
                        {
                            IsSuccess = false,
                            StatusCode = 400,
                            Message = $"Boarding service '{service.Name}' requires ScheduledEndAt.",
                            Result = null
                        };
                    }

                    if (itemRequest.ScheduledEndAt.Value.Date < itemRequest.ScheduledStartAt.Date)
                    {
                        return new ApiResponse<CreateBookingResponseDTO>
                        {
                            IsSuccess = false,
                            StatusCode = 400,
                            Message = $"Boarding service'{service.Name}' requires Checkout date must be same or after Checkin date.",
                            Result = null
                        };
                    }

                    if (itemRequest.ScheduledEndAt.Value.Date == itemRequest.ScheduledStartAt.Date)
                    {
                        // Case boarding in one day (Daycare): Checkin 08:00 - Checkout 18:00
                        itemStartAt = itemRequest.ScheduledStartAt.Date.AddHours(8);
                        itemEndAt = itemRequest.ScheduledStartAt.Date.AddHours(18);
                        durationDays = 1;
                    }
                    else
                    {
                        // Case boarding overnight: Checkin 14:00 - Checkout before 12:00
                        itemStartAt = itemRequest.ScheduledStartAt.Date.AddHours(14);
                        itemEndAt = itemRequest.ScheduledEndAt.Value.Date.AddHours(12);
                        durationDays = (decimal)(itemRequest.ScheduledEndAt.Value.Date - itemRequest.ScheduledStartAt.Date).TotalDays;
                    }

                    // Automatically define room types from service
                    if (!service.RoomTypeId.HasValue)
                    {
                        return new ApiResponse<CreateBookingResponseDTO>
                        {
                            IsSuccess = false,
                            StatusCode = 400,
                            Message = $"Boarding service '{service.Name}' has not been linked to any Room Type.",
                            Result = null
                        };
                    }

                    // Automatically find and lock 1 available room based on room type of service
                    var occupiedRoomIds = await _bookingItemRepository.GetQueryable()
                        .Where(bi => bi.RoomId.HasValue &&
                                     bi.Status != BookingItemStatus.Cancelled &&
                                     bi.Status != BookingItemStatus.No_Show &&
                                     bi.ScheduledStartAt < itemEndAt.Value &&
                                     (bi.ScheduledEndAt == null ? bi.ScheduledStartAt.AddDays(1) : bi.ScheduledEndAt.Value) > itemStartAt)
                        .Select(bi => bi.RoomId!.Value)
                        .Distinct()
                        .ToListAsync(cancellationToken);

                    var availableRoom = await _roomRepository.GetQueryable()
                        .Where(r => r.RoomTypeId == service.RoomTypeId.Value &&
                                    r.Status == RoomStatus.Available &&
                                    !occupiedRoomIds.Contains(r.Id) &&
                                    !allocatedRoomIdsInThisBooking.Contains(r.Id))
                        .FirstOrDefaultAsync(cancellationToken);

                    if (availableRoom == null)
                    {
                        return new ApiResponse<CreateBookingResponseDTO>
                        {
                            IsSuccess = false,
                            StatusCode = 400,
                            Message = $"No available rooms found for service '{service.Name}' during the requested period.",
                            Result = null
                        };
                    }

                    assignedRoomId = availableRoom.Id;
                    allocatedRoomIdsInThisBooking.Add(availableRoom.Id);
                }
                else if (service.ServiceType == ServiceType.Diet)
                {
                    if (itemEndAt.HasValue && itemEndAt.Value.Date >= itemStartAt.Date)
                    {
                        durationDays = (itemEndAt.Value.Date == itemStartAt.Date)
                            ? 1
                            : (decimal)(itemEndAt.Value.Date - itemStartAt.Date).TotalDays;
                    }
                    else
                    {
                        durationDays = Math.Max(1m, itemRequest.Quantity);
                    }
                }

                decimal finalQuantity = itemRequest.Quantity;
                if (service.ServiceType == ServiceType.Boarding || service.ServiceType == ServiceType.Diet)
                {
                    finalQuantity = (itemRequest.Quantity > 1 && itemRequest.Quantity != durationDays)
                        ? itemRequest.Quantity * durationDays
                        : durationDays;
                }

                var assignedPrice = unitPrice.Value * finalQuantity;
                subtotal += assignedPrice;

                newBooking.BookingItems.Add(new BookingItem
                {
                    Id = Guid.NewGuid(),
                    PetId = itemRequest.PetId,
                    ServiceId = itemRequest.ServiceId,
                    RoomId = assignedRoomId,
                    StaffId = null,
                    ScheduledStartAt = itemStartAt,
                    ScheduledEndAt = itemEndAt,
                    Quantity = finalQuantity,
                    UnitPrice = unitPrice.Value,
                    AssignedPrice = assignedPrice,
                    Status = BookingItemStatus.Pending
                });
            }

            // 6. Calculate total price with discount
            if (appliedVoucher != null)
            {
                discountAmount = appliedVoucher.DiscountType == DiscountType.Percentage
                    ? subtotal * (appliedVoucher.DiscountValue / 100)
                    : appliedVoucher.DiscountValue;

                appliedVoucher.CurrentUsage += 1;
                _voucherRepository.Update(appliedVoucher);
            }

            newBooking.TotalPrice = Math.Max(0, subtotal - discountAmount);

            // 7. Save booking
            await _bookingRepository.AddAsync(newBooking);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 8. Generate VNPay Payment URL if customer selected VNPAY option
            string? paymentUrl = null;
            paymentChoice = request.PaymentChoice?.Trim().ToUpper() ?? "PAY_AT_COUNTER";

            if (paymentChoice == "VNPAY" && newBooking.TotalPrice > 0)
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var ipAddress = httpContext?.Connection?.RemoteIpAddress?.ToString() ?? "127.0.0.1";
                paymentUrl = _vnPayService.CreatePaymentUrl(
                    newBooking.Id,
                    newBooking.TotalPrice,
                    $"Pay for booking with Id {newBooking.Id}",
                    ipAddress
                );
            }

            return new ApiResponse<CreateBookingResponseDTO>
            {
                IsSuccess = true,
                StatusCode = 201,
                Message = (paymentUrl != null)
                    ? "Booking created successfully. Please proceed to payment via the provided VNPay URL."
                    : "Booking created successfully with payment at counter.",
                Result = new CreateBookingResponseDTO
                {
                    BookingId = newBooking.Id,
                    TotalPrice = newBooking.TotalPrice,
                    PaymentChoice = paymentChoice,
                    PaymentUrl = paymentUrl
                }
            };
        }

        private decimal? CalculateUnitPrice(Service service, decimal petWeight)
        {
            var applicablePrice = service.ServicePrices
                .FirstOrDefault(p =>
                    (p.MinWeight == null || p.MinWeight <= petWeight) &&
                    (p.MaxWeight == null || p.MaxWeight >= petWeight));

            return applicablePrice?.Price;
        }
    }
}
