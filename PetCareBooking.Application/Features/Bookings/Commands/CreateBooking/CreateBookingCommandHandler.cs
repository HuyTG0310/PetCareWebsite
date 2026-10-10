using MediatR;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.Features.Bookings.Commands.CreateBooking
{
    public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, ApiResponse<Guid>>
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
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<Guid>> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
        {
            // 0. Xác thực & phân quyền: Customer bắt buộc lấy Id từ JWT token để chống IDOR
            Guid targetCustomerId;

            if (!_currentUserService.IsAdminOrStaff)
            {
                if (!_currentUserService.UserId.HasValue)
                {
                    return new ApiResponse<Guid>
                    {
                        IsSuccess = false,
                        StatusCode = 401,
                        Message = "User is not authenticated.",
                        Result = Guid.Empty
                    };
                }

                targetCustomerId = _currentUserService.UserId.Value;
            }
            else
            {
                // Staff/Admin tạo hộ khách: Bắt buộc chỉ định CustomerId
                if (!request.CustomerId.HasValue || request.CustomerId.Value == Guid.Empty)
                {
                    return new ApiResponse<Guid>
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = "CustomerId is required when staff creates a booking on behalf of a customer.",
                        Result = Guid.Empty
                    };
                }

                targetCustomerId = request.CustomerId.Value;
            }

            // 1. Validate customer exists
            var customers = await _customerRepository.FindAsync(c => c.Id == targetCustomerId);
            var customer = customers.FirstOrDefault();

            if (customer == null)
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = 404,
                    Message = "Customer not found.",
                    Result = Guid.Empty
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
                    return new ApiResponse<Guid>
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = $"Pet with ID {petId} not found.",
                        Result = Guid.Empty
                    };
                }

                if (pet.CustomerId != targetCustomerId)
                {
                    return new ApiResponse<Guid>
                    {
                        IsSuccess = false,
                        StatusCode = 403,
                        Message = $"Pet with ID {petId} does not belong to this customer.",
                        Result = Guid.Empty
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
                    return new ApiResponse<Guid>
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = $"Service with ID {serviceId} not found.",
                        Result = Guid.Empty
                    };
                }

                if (!service.IsActive)
                {
                    return new ApiResponse<Guid>
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = $"Service {service.Name} is not active.",
                        Result = Guid.Empty
                    };
                }
            }

            // 4. Validate and apply voucher
            Guid? voucherId = null;
            decimal discountAmount = 0;
            Voucher? appliedVoucher = null;

            if (!string.IsNullOrEmpty(request.VoucherCode))
            {
                var vouchers = await _voucherRepository.FindAsync(v =>
                    v.Code == request.VoucherCode &&
                    v.StartDate <= DateTime.UtcNow &&
                    v.EndDate >= DateTime.UtcNow);

                var voucher = vouchers.FirstOrDefault();

                if (voucher == null)
                {
                    return new ApiResponse<Guid>
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = "Invalid or expired voucher code.",
                        Result = Guid.Empty
                    };
                }

                if (voucher.MaxUsage.HasValue && voucher.CurrentUsage >= voucher.MaxUsage.Value)
                {
                    return new ApiResponse<Guid>
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = "Voucher usage limit has been reached.",
                        Result = Guid.Empty
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
                    return new ApiResponse<Guid>
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = $"No pricing found for {service.Name} with pet weight {pet.Weight}kg.",
                        Result = Guid.Empty
                    };
                }

                // XỬ LÝ PHÒNG CHO DỊCH VỤ BOARDING (LƯU TRÚ)
                Guid? assignedRoomId = null;
                if (service.ServiceType == ServiceType.Boarding)
                {
                    if (!itemRequest.ScheduledEndAt.HasValue || itemRequest.ScheduledEndAt.Value <= itemRequest.ScheduledStartAt)
                    {
                        return new ApiResponse<Guid>
                        {
                            IsSuccess = false,
                            StatusCode = 400,
                            Message = $"Boarding service '{service.Name}' requires a valid ScheduledEndAt after ScheduledStartAt.",
                            Result = Guid.Empty
                        };
                    }

                    // Tự động xác định loại phòng từ Dịch vụ (đã được liên kết trong DB)
                    if (!service.RoomTypeId.HasValue)
                    {
                        return new ApiResponse<Guid>
                        {
                            IsSuccess = false,
                            StatusCode = 400,
                            Message = $"Boarding service '{service.Name}' has not been linked to any Room Type.",
                            Result = Guid.Empty
                        };
                    }

                    // Hệ thống tự động tìm và khóa 1 phòng trống đúng loại phòng của gói dịch vụ
                    var occupiedRoomIds = await _bookingItemRepository.GetQueryable()
                        .Where(bi => bi.RoomId.HasValue &&
                                     bi.Status != BookingItemStatus.Cancelled &&
                                     bi.ScheduledStartAt < itemRequest.ScheduledEndAt.Value &&
                                     (bi.ScheduledEndAt == null ? bi.ScheduledStartAt.AddDays(1) : bi.ScheduledEndAt.Value) > itemRequest.ScheduledStartAt)
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
                        return new ApiResponse<Guid>
                        {
                            IsSuccess = false,
                            StatusCode = 400,
                            Message = $"No available rooms found for service '{service.Name}' during the requested period.",
                            Result = Guid.Empty
                        };
                    }

                    assignedRoomId = availableRoom.Id;
                    allocatedRoomIdsInThisBooking.Add(availableRoom.Id);
                }

                var assignedPrice = unitPrice.Value * itemRequest.Quantity;
                subtotal += assignedPrice;

                newBooking.BookingItems.Add(new BookingItem
                {
                    Id = Guid.NewGuid(),
                    PetId = itemRequest.PetId,
                    ServiceId = itemRequest.ServiceId,
                    RoomId = assignedRoomId,
                    StaffId = null,
                    ScheduledStartAt = itemRequest.ScheduledStartAt,
                    ScheduledEndAt = itemRequest.ScheduledEndAt,
                    Quantity = itemRequest.Quantity,
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

            return new ApiResponse<Guid>
            {
                IsSuccess = true,
                StatusCode = 201,
                Message = "Booking created successfully.",
                Result = newBooking.Id
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