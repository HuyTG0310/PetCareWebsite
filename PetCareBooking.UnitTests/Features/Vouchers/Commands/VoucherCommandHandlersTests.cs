using MockQueryable;
using MockQueryable.Moq;
using Moq;
using PetCareBooking.Application.Features.Vouchers.Commands.CreateVoucher;
using PetCareBooking.Application.Features.Vouchers.Commands.DeleteVoucher;
using PetCareBooking.Application.Features.Vouchers.Commands.UpdateVoucher;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;
using PetCareBooking.Domain.Enums;
using Xunit;

namespace PetCareBooking.UnitTests.Features.Vouchers.Commands
{
    public class VoucherCommandHandlersTests
    {
        private readonly Mock<IGenericRepository<Voucher>> _voucherRepoMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

        [Fact]
        public async Task CreateVoucher_WhenCodeAlreadyExists_Returns400BadRequest()
        {
            var existingVouchers = new List<Voucher>
            {
                new() { Id = Guid.NewGuid(), Code = "WELCOME10" }
            };

            var mockDbSet = existingVouchers.BuildMock();
            _voucherRepoMock.Setup(r => r.GetQueryable()).Returns(mockDbSet);

            var handler = new CreateVoucherCommandHandler(_voucherRepoMock.Object, _unitOfWorkMock.Object);
            var command = new CreateVoucherCommand
            {
                Code = "welcome10", // test case-insensitivity
                DiscountType = DiscountType.Percentage,
                DiscountValue = 10,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(10)
            };

            var response = await handler.Handle(command, CancellationToken.None);

            Assert.False(response.IsSuccess);
            Assert.Equal(400, response.StatusCode);
            Assert.Contains("already exists", response.Message);
            _voucherRepoMock.Verify(r => r.AddAsync(It.IsAny<Voucher>()), Times.Never);
        }

        [Fact]
        public async Task CreateVoucher_WhenValid_Returns201CreatedAndSetsCurrentUsageZero()
        {
            var emptyVouchers = new List<Voucher>();
            var mockDbSet = emptyVouchers.BuildMock();
            _voucherRepoMock.Setup(r => r.GetQueryable()).Returns(mockDbSet);

            Voucher? savedVoucher = null;
            _voucherRepoMock.Setup(r => r.AddAsync(It.IsAny<Voucher>()))
                            .Callback<Voucher>(v => savedVoucher = v)
                            .Returns(Task.CompletedTask);

            var handler = new CreateVoucherCommandHandler(_voucherRepoMock.Object, _unitOfWorkMock.Object);
            var command = new CreateVoucherCommand
            {
                Code = "newcode2026",
                DiscountType = DiscountType.Fixed_Amount,
                DiscountValue = 50000,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(5),
                MaxUsage = 50
            };

            var response = await handler.Handle(command, CancellationToken.None);

            Assert.True(response.IsSuccess);
            Assert.Equal(201, response.StatusCode);
            Assert.NotNull(savedVoucher);
            Assert.Equal("NEWCODE2026", savedVoucher.Code);
            Assert.Equal(0, savedVoucher.CurrentUsage);
            Assert.Equal(50, savedVoucher.MaxUsage);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateVoucher_WhenNotFound_Returns404()
        {
            var id = Guid.NewGuid();
            _voucherRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Voucher?)null);

            var handler = new UpdateVoucherCommandHandler(_voucherRepoMock.Object, _unitOfWorkMock.Object);
            var command = new UpdateVoucherCommand { Id = id, Code = "TEST" };

            var response = await handler.Handle(command, CancellationToken.None);

            Assert.False(response.IsSuccess);
            Assert.Equal(404, response.StatusCode);
            _voucherRepoMock.Verify(r => r.Update(It.IsAny<Voucher>()), Times.Never);
        }

        [Fact]
        public async Task UpdateVoucher_WhenDuplicateCodeWithAnotherVoucher_Returns400()
        {
            var id = Guid.NewGuid();
            var anotherId = Guid.NewGuid();

            var currentVoucher = new Voucher { Id = id, Code = "CURRENTCODE" };
            _voucherRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(currentVoucher);

            var existingVouchers = new List<Voucher>
            {
                currentVoucher,
                new() { Id = anotherId, Code = "EXISTINGCODE" }
            };
            var mockDbSet = existingVouchers.BuildMock();
            _voucherRepoMock.Setup(r => r.GetQueryable()).Returns(mockDbSet);

            var handler = new UpdateVoucherCommandHandler(_voucherRepoMock.Object, _unitOfWorkMock.Object);
            var command = new UpdateVoucherCommand
            {
                Id = id,
                Code = "existingcode", // Trùng với anotherId
                DiscountType = DiscountType.Percentage,
                DiscountValue = 15,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(3)
            };

            var response = await handler.Handle(command, CancellationToken.None);

            Assert.False(response.IsSuccess);
            Assert.Equal(400, response.StatusCode);
            Assert.Contains("already exists", response.Message);
            _voucherRepoMock.Verify(r => r.Update(It.IsAny<Voucher>()), Times.Never);
        }

        [Fact]
        public async Task UpdateVoucher_WhenValid_UpdatesFieldsAndReturns200()
        {
            var id = Guid.NewGuid();
            var currentVoucher = new Voucher
            {
                Id = id,
                Code = "OLDCODE",
                DiscountType = DiscountType.Percentage,
                DiscountValue = 10,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(5)
            };
            _voucherRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(currentVoucher);

            var existingVouchers = new List<Voucher> { currentVoucher };
            var mockDbSet = existingVouchers.BuildMock();
            _voucherRepoMock.Setup(r => r.GetQueryable()).Returns(mockDbSet);

            var handler = new UpdateVoucherCommandHandler(_voucherRepoMock.Object, _unitOfWorkMock.Object);
            var command = new UpdateVoucherCommand
            {
                Id = id,
                Code = "NEWCODE",
                DiscountType = DiscountType.Fixed_Amount,
                DiscountValue = 30000,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(10),
                MaxUsage = 100
            };

            var response = await handler.Handle(command, CancellationToken.None);

            Assert.True(response.IsSuccess);
            Assert.Equal(200, response.StatusCode);
            Assert.Equal("NEWCODE", currentVoucher.Code);
            Assert.Equal(DiscountType.Fixed_Amount, currentVoucher.DiscountType);
            Assert.Equal(30000, currentVoucher.DiscountValue);
            Assert.Equal(100, currentVoucher.MaxUsage);
            Assert.NotNull(currentVoucher.UpdatedAt);
            _voucherRepoMock.Verify(r => r.Update(currentVoucher), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteVoucher_WhenNotFound_Returns404()
        {
            var id = Guid.NewGuid();
            _voucherRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Voucher?)null);

            var handler = new DeleteVoucherCommandHandler(_voucherRepoMock.Object, _unitOfWorkMock.Object);

            var response = await handler.Handle(new DeleteVoucherCommand(id), CancellationToken.None);

            Assert.False(response.IsSuccess);
            Assert.Equal(404, response.StatusCode);
        }

        [Fact]
        public async Task DeleteVoucher_WhenCurrentUsageIsZero_ExecutesHardDelete()
        {
            // Business Rule: Nếu CurrentUsage == 0 thì Hard Delete
            var id = Guid.NewGuid();
            var unusedVoucher = new Voucher
            {
                Id = id,
                Code = "UNUSED",
                CurrentUsage = 0
            };
            _voucherRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(unusedVoucher);

            var handler = new DeleteVoucherCommandHandler(_voucherRepoMock.Object, _unitOfWorkMock.Object);

            var response = await handler.Handle(new DeleteVoucherCommand(id), CancellationToken.None);

            Assert.True(response.IsSuccess);
            Assert.Equal(200, response.StatusCode);
            Assert.Contains("permanently deleted", response.Message);
            _voucherRepoMock.Verify(r => r.Delete(unusedVoucher), Times.Once);
            _voucherRepoMock.Verify(r => r.Update(It.IsAny<Voucher>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteVoucher_WhenCurrentUsageGreaterThanZero_DeactivatesEndDateInsteadOfHardDelete()
        {
            // Business Rule: Nếu CurrentUsage > 0 thì KHÔNG được xóa cứng mà set EndDate = DateTime.UtcNow
            var id = Guid.NewGuid();
            var futureDate = DateTime.UtcNow.AddDays(30);
            var usedVoucher = new Voucher
            {
                Id = id,
                Code = "USED100",
                CurrentUsage = 5,
                EndDate = futureDate
            };
            _voucherRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(usedVoucher);

            var handler = new DeleteVoucherCommandHandler(_voucherRepoMock.Object, _unitOfWorkMock.Object);

            var response = await handler.Handle(new DeleteVoucherCommand(id), CancellationToken.None);

            Assert.True(response.IsSuccess);
            Assert.Equal(200, response.StatusCode);
            Assert.Contains("deactivated", response.Message);
            Assert.True(usedVoucher.EndDate <= DateTime.UtcNow.AddSeconds(1));
            Assert.NotNull(usedVoucher.UpdatedAt);
            _voucherRepoMock.Verify(r => r.Delete(It.IsAny<Voucher>()), Times.Never); // Không gọi Delete!
            _voucherRepoMock.Verify(r => r.Update(usedVoucher), Times.Once);          // Gọi Update!
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
