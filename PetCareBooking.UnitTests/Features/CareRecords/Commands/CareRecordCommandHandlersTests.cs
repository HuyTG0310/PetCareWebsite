using Moq;
using PetCareBooking.Application.Features.CareRecords.Commands.CreateCareRecord;
using PetCareBooking.Application.Features.CareRecords.Commands.DeleteCareRecord;
using PetCareBooking.Application.Features.CareRecords.Commands.UpdateCareRecord;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;
using Xunit;

namespace PetCareBooking.UnitTests.Features.CareRecords.Commands
{
    public class CareRecordCommandHandlersTests
    {
        private readonly Mock<IGenericRepository<CareRecord>> _careRecordRepoMock = new();
        private readonly Mock<IGenericRepository<BookingItem>> _bookingItemRepoMock = new();
        private readonly Mock<IGenericRepository<Staff>> _staffRepoMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

        [Fact]
        public async Task CreateCareRecord_WhenBookingItemNotFound_Returns404()
        {
            var bookingItemId = Guid.NewGuid();
            _bookingItemRepoMock.Setup(r => r.GetByIdAsync(bookingItemId)).ReturnsAsync((BookingItem?)null);

            var handler = new CreateCareRecordCommandHandler(
                _careRecordRepoMock.Object,
                _bookingItemRepoMock.Object,
                _staffRepoMock.Object,
                _unitOfWorkMock.Object);

            var command = new CreateCareRecordCommand
            {
                BookingItemId = bookingItemId,
                StaffId = Guid.NewGuid(),
                RecordDate = DateTime.UtcNow
            };

            var response = await handler.Handle(command, CancellationToken.None);

            Assert.False(response.IsSuccess);
            Assert.Equal(404, response.StatusCode);
            Assert.Equal("Booking item not found.", response.Message);
            _careRecordRepoMock.Verify(r => r.AddAsync(It.IsAny<CareRecord>()), Times.Never);
        }

        [Fact]
        public async Task CreateCareRecord_WhenStaffNotFound_Returns404()
        {
            var bookingItemId = Guid.NewGuid();
            var staffId = Guid.NewGuid();

            _bookingItemRepoMock.Setup(r => r.GetByIdAsync(bookingItemId))
                                .ReturnsAsync(new BookingItem { Id = bookingItemId });
            _staffRepoMock.Setup(r => r.GetByIdAsync(staffId))
                          .ReturnsAsync((Staff?)null);

            var handler = new CreateCareRecordCommandHandler(
                _careRecordRepoMock.Object,
                _bookingItemRepoMock.Object,
                _staffRepoMock.Object,
                _unitOfWorkMock.Object);

            var command = new CreateCareRecordCommand
            {
                BookingItemId = bookingItemId,
                StaffId = staffId,
                RecordDate = DateTime.UtcNow
            };

            var response = await handler.Handle(command, CancellationToken.None);

            Assert.False(response.IsSuccess);
            Assert.Equal(404, response.StatusCode);
            Assert.Equal("Staff not found.", response.Message);
            _careRecordRepoMock.Verify(r => r.AddAsync(It.IsAny<CareRecord>()), Times.Never);
        }

        [Fact]
        public async Task CreateCareRecord_WhenValid_Returns201Created()
        {
            var bookingItemId = Guid.NewGuid();
            var staffId = Guid.NewGuid();

            _bookingItemRepoMock.Setup(r => r.GetByIdAsync(bookingItemId))
                                .ReturnsAsync(new BookingItem { Id = bookingItemId });
            _staffRepoMock.Setup(r => r.GetByIdAsync(staffId))
                          .ReturnsAsync(new Staff { Id = staffId, FullName = "Staff Nguyen" });

            CareRecord? savedRecord = null;
            _careRecordRepoMock.Setup(r => r.AddAsync(It.IsAny<CareRecord>()))
                               .Callback<CareRecord>(c => savedRecord = c)
                               .Returns(Task.CompletedTask);

            var handler = new CreateCareRecordCommandHandler(
                _careRecordRepoMock.Object,
                _bookingItemRepoMock.Object,
                _staffRepoMock.Object,
                _unitOfWorkMock.Object);

            var command = new CreateCareRecordCommand
            {
                BookingItemId = bookingItemId,
                StaffId = staffId,
                RecordDate = DateTime.UtcNow,
                HealthStatus = "Khỏe mạnh",
                Note = "Ăn hết hạt"
            };

            var response = await handler.Handle(command, CancellationToken.None);

            Assert.True(response.IsSuccess);
            Assert.Equal(201, response.StatusCode);
            Assert.NotNull(savedRecord);
            Assert.Equal(bookingItemId, savedRecord.BookingItemId);
            Assert.Equal(staffId, savedRecord.StaffId);
            Assert.Equal("Khỏe mạnh", savedRecord.HealthStatus);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateCareRecord_WhenNotFound_Returns404()
        {
            var recordId = Guid.NewGuid();
            _careRecordRepoMock.Setup(r => r.GetByIdAsync(recordId)).ReturnsAsync((CareRecord?)null);

            var handler = new UpdateCareRecordCommandHandler(
                _careRecordRepoMock.Object,
                _unitOfWorkMock.Object);

            var command = new UpdateCareRecordCommand { Id = recordId, HealthStatus = "Mệt nhẹ" };

            var response = await handler.Handle(command, CancellationToken.None);

            Assert.False(response.IsSuccess);
            Assert.Equal(404, response.StatusCode);
            _careRecordRepoMock.Verify(r => r.Update(It.IsAny<CareRecord>()), Times.Never);
        }

        [Fact]
        public async Task UpdateCareRecord_WhenFound_UpdatesAndReturns200()
        {
            var recordId = Guid.NewGuid();
            var record = new CareRecord { Id = recordId, HealthStatus = "Bình thường" };
            _careRecordRepoMock.Setup(r => r.GetByIdAsync(recordId)).ReturnsAsync(record);

            var handler = new UpdateCareRecordCommandHandler(
                _careRecordRepoMock.Object,
                _unitOfWorkMock.Object);

            var command = new UpdateCareRecordCommand
            {
                Id = recordId,
                HealthStatus = "Đã hồi phục hoàn toàn",
                Note = "Đã uống thuốc"
            };

            var response = await handler.Handle(command, CancellationToken.None);

            Assert.True(response.IsSuccess);
            Assert.Equal(200, response.StatusCode);
            Assert.Equal("Đã hồi phục hoàn toàn", record.HealthStatus);
            Assert.Equal("Đã uống thuốc", record.Note);
            Assert.NotNull(record.UpdatedAt);
            _careRecordRepoMock.Verify(r => r.Update(record), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteCareRecord_WhenNotFound_Returns404()
        {
            var recordId = Guid.NewGuid();
            _careRecordRepoMock.Setup(r => r.GetByIdAsync(recordId)).ReturnsAsync((CareRecord?)null);

            var handler = new DeleteCareRecordCommandHandler(
                _careRecordRepoMock.Object,
                _unitOfWorkMock.Object);

            var response = await handler.Handle(new DeleteCareRecordCommand(recordId), CancellationToken.None);

            Assert.False(response.IsSuccess);
            Assert.Equal(404, response.StatusCode);
            _careRecordRepoMock.Verify(r => r.Delete(It.IsAny<CareRecord>()), Times.Never);
        }

        [Fact]
        public async Task DeleteCareRecord_WhenFound_DeletesAndReturns200()
        {
            var recordId = Guid.NewGuid();
            var record = new CareRecord { Id = recordId };
            _careRecordRepoMock.Setup(r => r.GetByIdAsync(recordId)).ReturnsAsync(record);

            var handler = new DeleteCareRecordCommandHandler(
                _careRecordRepoMock.Object,
                _unitOfWorkMock.Object);

            var response = await handler.Handle(new DeleteCareRecordCommand(recordId), CancellationToken.None);

            Assert.True(response.IsSuccess);
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.Result);
            _careRecordRepoMock.Verify(r => r.Delete(record), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
