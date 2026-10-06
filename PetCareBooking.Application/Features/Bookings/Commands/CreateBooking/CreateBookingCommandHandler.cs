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
        private readonly IGenericRepository<Promotion> _promotionRepository;
        private readonly IGenericRepository<ServicePrice> _servicePriceRepository;
        private readonly IGenericRepository<Room> _roomRepository;
        private readonly IGenericRepository<BookingItem> _bookingItemRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateBookingCommandHandler(
            IGenericRepository<Booking> bookingRepository,
            IGenericRepository<Customer> customerRepository,
            IGenericRepository<Pet> petRepository,
            IGenericRepository<Service> serviceRepository,
            IGenericRepository<Promotion> promotionRepository,
            IGenericRepository<ServicePrice> servicePriceRepository,
            IGenericRepository<Room> roomRepository,
            IGenericRepository<BookingItem> bookingItemRepository,
            IUnitOfWork unitOfWork)
        {
            _bookingRepository = bookingRepository;
            _customerRepository = customerRepository;
            _petRepository = petRepository;
            _serviceRepository = serviceRepository;
            _promotionRepository = promotionRepository;
            _servicePriceRepository = servicePriceRepository;
            _roomRepository = roomRepository;
            _bookingItemRepository = bookingItemRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<Guid>> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
        {
            // 1. Validate customer exists
            var customers = await _customerRepository.FindAsync(c => c.Id == request.CustomerId);
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

                if (pet.CustomerId != request.CustomerId)
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
            var services = await _serviceRepository.FindAsync(s => serviceIds.Contains(s.Id), "ServicePrices");

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

            // 4. Validate and apply promotion
            Guid? promotionId = null;
            decimal discountAmount = 0;

            if (!string.IsNullOrEmpty(request.PromotionCode))
            {
                var promotions = await _promotionRepository.FindAsync(p =>
                    p.Code == request.PromotionCode &&
                    p.StartDate <= DateTime.UtcNow &&
                    p.EndDate >= DateTime.UtcNow);

                var promotion = promotions.FirstOrDefault();

                if (promotion == null)
                {
                    return new ApiResponse<Guid>
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = "Invalid or expired promotion code.",
                        Result = Guid.Empty
                    };
                }

                promotionId = promotion.Id;
            }

            // 5. Create booking with items and calculate prices
            var newBooking = new Booking
            {
                Id = Guid.NewGuid(),
                CustomerId = request.CustomerId,
                PromotionId = promotionId,
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

                    // Trường hợp 1: Khách hoặc nhân viên chỉ định phòng cụ thể (RoomId)
                    if (itemRequest.RoomId.HasValue)
                    {
                        var room = await _roomRepository.GetByIdAsync(itemRequest.RoomId.Value);
                        if (room == null)
                        {
                            return new ApiResponse<Guid>
                            {
                                IsSuccess = false,
                                StatusCode = 404,
                                Message = $"Room with ID {itemRequest.RoomId.Value} not found.",
                                Result = Guid.Empty
                            };
                        }

                        if (room.Status != RoomStatus.Available)
                        {
                            return new ApiResponse<Guid>
                            {
                                IsSuccess = false,
                                StatusCode = 400,
                                Message = $"Room '{room.RoomName}' is currently under maintenance.",
                                Result = Guid.Empty
                            };
                        }

                        var isOccupied = await _bookingItemRepository.GetQueryable()
                            .AnyAsync(bi => bi.RoomId == itemRequest.RoomId.Value &&
                                            bi.Status != BookingItemStatus.Cancelled &&
                                            bi.ScheduledStartAt < itemRequest.ScheduledEndAt.Value &&
                                            (bi.ScheduledEndAt == null ? bi.ScheduledStartAt.AddDays(1) : bi.ScheduledEndAt.Value) > itemRequest.ScheduledStartAt,
                                      cancellationToken);

                        if (isOccupied || allocatedRoomIdsInThisBooking.Contains(itemRequest.RoomId.Value))
                        {
                            return new ApiResponse<Guid>
                            {
                                IsSuccess = false,
                                StatusCode = 400,
                                Message = $"Room '{room.RoomName}' is already occupied during the requested period.",
                                Result = Guid.Empty
                            };
                        }

                        assignedRoomId = room.Id;
                        allocatedRoomIdsInThisBooking.Add(room.Id);
                    }
                    // Trường hợp 2: Khách chọn Loại phòng (RoomTypeId) -> Hệ thống tự động tìm và khóa 1 phòng trống
                    else if (itemRequest.RoomTypeId.HasValue)
                    {
                        var occupiedRoomIds = await _bookingItemRepository.GetQueryable()
                            .Where(bi => bi.RoomId.HasValue &&
                                         bi.Status != BookingItemStatus.Cancelled &&
                                         bi.ScheduledStartAt < itemRequest.ScheduledEndAt.Value &&
                                         (bi.ScheduledEndAt == null ? bi.ScheduledStartAt.AddDays(1) : bi.ScheduledEndAt.Value) > itemRequest.ScheduledStartAt)
                            .Select(bi => bi.RoomId!.Value)
                            .Distinct()
                            .ToListAsync(cancellationToken);

                        var availableRoom = await _roomRepository.GetQueryable()
                            .Where(r => r.RoomTypeId == itemRequest.RoomTypeId.Value &&
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
                                Message = "No available rooms found for the selected room type during the requested period.",
                                Result = Guid.Empty
                            };
                        }

                        assignedRoomId = availableRoom.Id;
                        allocatedRoomIdsInThisBooking.Add(availableRoom.Id);
                    }
                    else
                    {
                        return new ApiResponse<Guid>
                        {
                            IsSuccess = false,
                            StatusCode = 400,
                            Message = $"Boarding service '{service.Name}' requires selecting either a Room or a Room Type.",
                            Result = Guid.Empty
                        };
                    }
                }

                var assignedPrice = unitPrice.Value * itemRequest.Quantity;
                subtotal += assignedPrice;

                newBooking.BookingItems.Add(new BookingItem
                {
                    Id = Guid.NewGuid(),
                    PetId = itemRequest.PetId,
                    ServiceId = itemRequest.ServiceId,
                    RoomId = assignedRoomId,
                    StaffId = itemRequest.StaffId,
                    ScheduledStartAt = itemRequest.ScheduledStartAt,
                    ScheduledEndAt = itemRequest.ScheduledEndAt,
                    Quantity = itemRequest.Quantity,
                    UnitPrice = unitPrice.Value,
                    AssignedPrice = assignedPrice,
                    Status = BookingItemStatus.Pending
                });
            }

            // 6. Calculate total price with discount
            if (promotionId.HasValue)
            {
                var promotion = (await _promotionRepository.FindAsync(p => p.Id == promotionId)).First();
                discountAmount = promotion.DiscountType == DiscountType.Percentage
                    ? subtotal * (promotion.DiscountValue / 100)
                    : promotion.DiscountValue;
            }

            newBooking.TotalPrice = subtotal - discountAmount;

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