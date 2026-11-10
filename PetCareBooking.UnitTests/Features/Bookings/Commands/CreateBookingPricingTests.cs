using System.Linq.Expressions;
using Moq;
using PetCareBooking.Application.DTOs.Booking;
using PetCareBooking.Application.Features.Bookings.Commands.CreateBooking;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;
using PetCareBooking.Domain.Enums;
using Xunit;

namespace PetCareBooking.UnitTests.Features.Bookings.Commands
{
    public class CreateBookingPricingTests
    {
        private readonly Mock<IGenericRepository<Booking>> _bookingRepoMock = new();
        private readonly Mock<IGenericRepository<Customer>> _customerRepoMock = new();
        private readonly Mock<IGenericRepository<Pet>> _petRepoMock = new();
        private readonly Mock<IGenericRepository<Service>> _serviceRepoMock = new();
        private readonly Mock<IGenericRepository<Voucher>> _voucherRepoMock = new();
        private readonly Mock<IGenericRepository<ServicePrice>> _servicePriceRepoMock = new();
        private readonly Mock<IGenericRepository<Room>> _roomRepoMock = new();
        private readonly Mock<IGenericRepository<BookingItem>> _bookingItemRepoMock = new();
        private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

        private readonly Guid _customerId = Guid.NewGuid();
        private readonly Guid _petId = Guid.NewGuid();
        private readonly Guid _serviceId = Guid.NewGuid();

        public CreateBookingPricingTests()
        {
            // Setup authenticated customer
            _currentUserServiceMock.Setup(s => s.IsAdminOrStaff).Returns(false);
            _currentUserServiceMock.Setup(s => s.UserId).Returns(_customerId);

            // Setup Customer
            var customer = new Customer { Id = _customerId, FullName = "Nguyễn Văn A" };
            _customerRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Customer, bool>>>()))
                             .ReturnsAsync(new List<Customer> { customer });

            // Setup Pet
            var pet = new Pet
            {
                Id = _petId,
                CustomerId = _customerId,
                Name = "Miu",
                Weight = 5.0m,
                IsActive = true
            };
            _petRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Pet, bool>>>()))
                        .ReturnsAsync(new List<Pet> { pet });

            // Setup Service with Pricing
            var service = new Service
            {
                Id = _serviceId,
                Name = "Tắm spa cho mèo",
                ServiceType = ServiceType.Grooming,
                IsActive = true,
                ServicePrices = new List<ServicePrice>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ServiceId = _serviceId,
                        MinWeight = 0,
                        MaxWeight = 10,
                        Price = 200000m // 200,000 VNĐ
                    }
                }
            };
            _serviceRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Service, bool>>>(), It.IsAny<string[]>()))
                            .ReturnsAsync(new List<Service> { service });
        }

        private CreateBookingCommandHandler CreateHandler()
        {
            return new CreateBookingCommandHandler(
                _bookingRepoMock.Object,
                _customerRepoMock.Object,
                _petRepoMock.Object,
                _serviceRepoMock.Object,
                _voucherRepoMock.Object,
                _servicePriceRepoMock.Object,
                _roomRepoMock.Object,
                _bookingItemRepoMock.Object,
                _currentUserServiceMock.Object,
                _unitOfWorkMock.Object
            );
        }

        [Fact]
        public async Task Handle_WithoutVoucher_CalculatesSubtotalCorrectly()
        {
            // Quantity = 2 -> 200,000 * 2 = 400,000 VNĐ
            Booking? savedBooking = null;
            _bookingRepoMock.Setup(r => r.AddAsync(It.IsAny<Booking>()))
                            .Callback<Booking>(b => savedBooking = b)
                            .Returns(Task.CompletedTask);

            var command = new CreateBookingCommand
            {
                BookingItems = new List<BookingItemRequestDTO>
                {
                    new()
                    {
                        PetId = _petId,
                        ServiceId = _serviceId,
                        Quantity = 2,
                        ScheduledStartAt = DateTime.UtcNow.AddDays(1)
                    }
                }
            };

            var handler = CreateHandler();
            var response = await handler.Handle(command, CancellationToken.None);

            Assert.True(response.IsSuccess);
            Assert.Equal(201, response.StatusCode);
            Assert.NotNull(savedBooking);
            Assert.Equal(400000m, savedBooking.TotalPrice); // 200,000 * 2 = 400,000
            Assert.Null(savedBooking.VoucherId);
        }

        [Fact]
        public async Task Handle_WithPercentageVoucher_CalculatesDiscountAndIncrementsUsage()
        {
            // Subtotal = 200,000 * 2 = 400,000 VNĐ
            // Voucher giảm 20% -> Discount = 80,000 VNĐ -> TotalPrice = 320,000 VNĐ
            var voucher = new Voucher
            {
                Id = Guid.NewGuid(),
                Code = "SALE20",
                DiscountType = DiscountType.Percentage,
                DiscountValue = 20m, // 20%
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(5),
                MaxUsage = 100,
                CurrentUsage = 0
            };

            _voucherRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Voucher, bool>>>()))
                            .ReturnsAsync(new List<Voucher> { voucher });

            Booking? savedBooking = null;
            _bookingRepoMock.Setup(r => r.AddAsync(It.IsAny<Booking>()))
                            .Callback<Booking>(b => savedBooking = b)
                            .Returns(Task.CompletedTask);

            var command = new CreateBookingCommand
            {
                VoucherCode = "SALE20",
                BookingItems = new List<BookingItemRequestDTO>
                {
                    new()
                    {
                        PetId = _petId,
                        ServiceId = _serviceId,
                        Quantity = 2,
                        ScheduledStartAt = DateTime.UtcNow.AddDays(1)
                    }
                }
            };

            var handler = CreateHandler();
            var response = await handler.Handle(command, CancellationToken.None);

            Assert.True(response.IsSuccess);
            Assert.Equal(201, response.StatusCode);
            Assert.NotNull(savedBooking);
            Assert.Equal(320000m, savedBooking.TotalPrice); // 400,000 - 80,000 = 320,000
            Assert.Equal(voucher.Id, savedBooking.VoucherId);
            Assert.Equal(1, voucher.CurrentUsage); // CurrentUsage tăng từ 0 lên 1
            _voucherRepoMock.Verify(r => r.Update(voucher), Times.Once);
        }

        [Fact]
        public async Task Handle_WithFixedAmountVoucher_CalculatesDiscountAndIncrementsUsage()
        {
            // Subtotal = 200,000 * 2 = 400,000 VNĐ
            // Voucher giảm 50,000 VNĐ -> TotalPrice = 350,000 VNĐ
            var voucher = new Voucher
            {
                Id = Guid.NewGuid(),
                Code = "FIXED50K",
                DiscountType = DiscountType.Fixed_Amount,
                DiscountValue = 50000m,
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(5),
                MaxUsage = 50,
                CurrentUsage = 5
            };

            _voucherRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Voucher, bool>>>()))
                            .ReturnsAsync(new List<Voucher> { voucher });

            Booking? savedBooking = null;
            _bookingRepoMock.Setup(r => r.AddAsync(It.IsAny<Booking>()))
                            .Callback<Booking>(b => savedBooking = b)
                            .Returns(Task.CompletedTask);

            var command = new CreateBookingCommand
            {
                VoucherCode = "FIXED50K",
                BookingItems = new List<BookingItemRequestDTO>
                {
                    new()
                    {
                        PetId = _petId,
                        ServiceId = _serviceId,
                        Quantity = 2,
                        ScheduledStartAt = DateTime.UtcNow.AddDays(1)
                    }
                }
            };

            var handler = CreateHandler();
            var response = await handler.Handle(command, CancellationToken.None);

            Assert.True(response.IsSuccess);
            Assert.Equal(201, response.StatusCode);
            Assert.NotNull(savedBooking);
            Assert.Equal(350000m, savedBooking.TotalPrice); // 400,000 - 50,000 = 350,000
            Assert.Equal(voucher.Id, savedBooking.VoucherId);
            Assert.Equal(6, voucher.CurrentUsage); // Tăng từ 5 lên 6
            _voucherRepoMock.Verify(r => r.Update(voucher), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenDiscountExceedsSubtotal_ClampsTotalPriceToZero()
        {
            // Subtotal = 200,000 * 1 = 200,000 VNĐ
            // Voucher giảm 500,000 VNĐ (lớn hơn subtotal)
            // TotalPrice = Math.Max(0, 200,000 - 500,000) = 0 VNĐ (Không bao giờ bị âm)
            var voucher = new Voucher
            {
                Id = Guid.NewGuid(),
                Code = "BIGVOUCHER",
                DiscountType = DiscountType.Fixed_Amount,
                DiscountValue = 500000m,
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(5),
                MaxUsage = 10,
                CurrentUsage = 0
            };

            _voucherRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Voucher, bool>>>()))
                            .ReturnsAsync(new List<Voucher> { voucher });

            Booking? savedBooking = null;
            _bookingRepoMock.Setup(r => r.AddAsync(It.IsAny<Booking>()))
                            .Callback<Booking>(b => savedBooking = b)
                            .Returns(Task.CompletedTask);

            var command = new CreateBookingCommand
            {
                VoucherCode = "BIGVOUCHER",
                BookingItems = new List<BookingItemRequestDTO>
                {
                    new()
                    {
                        PetId = _petId,
                        ServiceId = _serviceId,
                        Quantity = 1,
                        ScheduledStartAt = DateTime.UtcNow.AddDays(1)
                    }
                }
            };

            var handler = CreateHandler();
            var response = await handler.Handle(command, CancellationToken.None);

            Assert.True(response.IsSuccess);
            Assert.Equal(201, response.StatusCode);
            Assert.NotNull(savedBooking);
            Assert.Equal(0m, savedBooking.TotalPrice); // Không âm, được chặn ở 0
        }

        [Fact]
        public async Task Handle_WhenVoucherExpiredOrNotFound_Returns400BadRequest()
        {
            // Voucher repo không tìm thấy voucher còn hạn
            _voucherRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Voucher, bool>>>()))
                            .ReturnsAsync(new List<Voucher>());

            var command = new CreateBookingCommand
            {
                VoucherCode = "EXPIRED_CODE",
                BookingItems = new List<BookingItemRequestDTO>
                {
                    new()
                    {
                        PetId = _petId,
                        ServiceId = _serviceId,
                        Quantity = 1,
                        ScheduledStartAt = DateTime.UtcNow.AddDays(1)
                    }
                }
            };

            var handler = CreateHandler();
            var response = await handler.Handle(command, CancellationToken.None);

            Assert.False(response.IsSuccess);
            Assert.Equal(400, response.StatusCode);
            Assert.Contains("Invalid or expired voucher code", response.Message);
            _bookingRepoMock.Verify(r => r.AddAsync(It.IsAny<Booking>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenVoucherUsageLimitReached_Returns400BadRequest()
        {
            var voucher = new Voucher
            {
                Id = Guid.NewGuid(),
                Code = "MAXED_OUT",
                DiscountType = DiscountType.Percentage,
                DiscountValue = 10m,
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(5),
                MaxUsage = 10,
                CurrentUsage = 10 // Đã đạt giới hạn tối đa
            };

            _voucherRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Voucher, bool>>>()))
                            .ReturnsAsync(new List<Voucher> { voucher });

            var command = new CreateBookingCommand
            {
                VoucherCode = "MAXED_OUT",
                BookingItems = new List<BookingItemRequestDTO>
                {
                    new()
                    {
                        PetId = _petId,
                        ServiceId = _serviceId,
                        Quantity = 1,
                        ScheduledStartAt = DateTime.UtcNow.AddDays(1)
                    }
                }
            };

            var handler = CreateHandler();
            var response = await handler.Handle(command, CancellationToken.None);

            Assert.False(response.IsSuccess);
            Assert.Equal(400, response.StatusCode);
            Assert.Contains("usage limit has been reached", response.Message);
            _bookingRepoMock.Verify(r => r.AddAsync(It.IsAny<Booking>()), Times.Never);
        }
    }
}
