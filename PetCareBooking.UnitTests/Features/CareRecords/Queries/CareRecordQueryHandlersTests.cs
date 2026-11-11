using MockQueryable;
using MockQueryable.Moq;
using Moq;
using PetCareBooking.Application.Features.CareRecords.Queries.GetCareRecordById;
using PetCareBooking.Application.Features.CareRecords.Queries.GetCareRecordsWithPagination;
using PetCareBooking.Application.Features.CareRecords.Queries.SearchCareRecords;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;
using Xunit;

namespace PetCareBooking.UnitTests.Features.CareRecords.Queries
{
    public class CareRecordQueryHandlersTests
    {
        private readonly Mock<IGenericRepository<CareRecord>> _careRecordRepoMock = new();

        [Fact]
        public async Task GetCareRecordsWithPagination_ReturnsCorrectPagedResult()
        {
            var records = new List<CareRecord>
            {
                new() { Id = Guid.NewGuid(), BookingItemId = Guid.NewGuid(), StaffId = Guid.NewGuid(), RecordDate = DateTime.UtcNow.AddHours(-2), HealthStatus = "Khỏe" },
                new() { Id = Guid.NewGuid(), BookingItemId = Guid.NewGuid(), StaffId = Guid.NewGuid(), RecordDate = DateTime.UtcNow.AddHours(-1), HealthStatus = "Bình thường" },
                new() { Id = Guid.NewGuid(), BookingItemId = Guid.NewGuid(), StaffId = Guid.NewGuid(), RecordDate = DateTime.UtcNow, HealthStatus = "Tốt" }
            };

            var mockDbSet = records.BuildMock();
            _careRecordRepoMock.Setup(r => r.GetQueryable()).Returns(mockDbSet);

            var handler = new GetCareRecordsWithPaginationQueryHandler(_careRecordRepoMock.Object);
            var query = new GetCareRecordsWithPaginationQuery { PageIndex = 1, PageSize = 2 };

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(3, result.TotalCount);
            Assert.Equal(2, result.Items.Count);
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(2, result.PageSize);
        }

        [Fact]
        public async Task GetCareRecordById_WhenNotFound_Returns404()
        {
            var emptyList = new List<CareRecord>();
            var mockDbSet = emptyList.BuildMock();
            _careRecordRepoMock.Setup(r => r.GetQueryable()).Returns(mockDbSet);

            var handler = new GetCareRecordByIdQueryHandler(_careRecordRepoMock.Object);
            var query = new GetCareRecordByIdQuery(Guid.NewGuid());

            var response = await handler.Handle(query, CancellationToken.None);

            Assert.False(response.IsSuccess);
            Assert.Equal(404, response.StatusCode);
            Assert.Null(response.Result);
        }

        [Fact]
        public async Task GetCareRecordById_WhenFound_Returns200WithDetails()
        {
            var recordId = Guid.NewGuid();
            var staff = new Staff { Id = Guid.NewGuid(), FullName = "Bác Sĩ Nam" };
            var record = new CareRecord
            {
                Id = recordId,
                BookingItemId = Guid.NewGuid(),
                StaffId = staff.Id,
                Staff = staff,
                RecordDate = DateTime.UtcNow,
                HealthStatus = "Tốt",
                Note = "Ăn uống bình thường",
                ImageUrl = "http://example.com/pet.png"
            };

            var records = new List<CareRecord> { record };
            var mockDbSet = records.BuildMock();
            _careRecordRepoMock.Setup(r => r.GetQueryable()).Returns(mockDbSet);

            var handler = new GetCareRecordByIdQueryHandler(_careRecordRepoMock.Object);
            var query = new GetCareRecordByIdQuery(recordId);

            var response = await handler.Handle(query, CancellationToken.None);

            Assert.True(response.IsSuccess);
            Assert.Equal(200, response.StatusCode);
            Assert.NotNull(response.Result);
            Assert.Equal(recordId, response.Result.Id);
            Assert.Equal("Bác Sĩ Nam", response.Result.StaffName);
            Assert.Equal("Tốt", response.Result.HealthStatus);
        }

        [Fact]
        public async Task SearchCareRecords_ByKeyword_ReturnsMatching()
        {
            var records = new List<CareRecord>
            {
                new() { Id = Guid.NewGuid(), HealthStatus = "Sốt nhẹ", Note = "Theo dõi thêm", RecordDate = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), HealthStatus = "Khỏe mạnh", Note = "Bình thường", RecordDate = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), HealthStatus = "Bình thường", Note = "Có biểu hiện sốt", RecordDate = DateTime.UtcNow }
            };

            var mockDbSet = records.BuildMock();
            _careRecordRepoMock.Setup(r => r.GetQueryable()).Returns(mockDbSet);

            var handler = new SearchCareRecordsQueryHandler(_careRecordRepoMock.Object);
            var query = new SearchCareRecordsQuery { Keyword = "sốt", PageIndex = 1, PageSize = 10 };

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.Equal(2, result.TotalCount);
            Assert.Equal(2, result.Items.Count);
        }

        [Fact]
        public async Task SearchCareRecords_ByBookingItemIdAndStaffId_FiltersCorrectly()
        {
            var targetBookingItemId = Guid.NewGuid();
            var targetStaffId = Guid.NewGuid();

            var records = new List<CareRecord>
            {
                new() { Id = Guid.NewGuid(), BookingItemId = targetBookingItemId, StaffId = targetStaffId, RecordDate = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), BookingItemId = targetBookingItemId, StaffId = Guid.NewGuid(), RecordDate = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), BookingItemId = Guid.NewGuid(), StaffId = targetStaffId, RecordDate = DateTime.UtcNow }
            };

            var mockDbSet = records.BuildMock();
            _careRecordRepoMock.Setup(r => r.GetQueryable()).Returns(mockDbSet);

            var handler = new SearchCareRecordsQueryHandler(_careRecordRepoMock.Object);
            var query = new SearchCareRecordsQuery
            {
                BookingItemId = targetBookingItemId,
                StaffId = targetStaffId,
                PageIndex = 1,
                PageSize = 10
            };

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.Equal(1, result.TotalCount);
            Assert.Single(result.Items);
            Assert.Equal(targetBookingItemId, result.Items[0].BookingItemId);
            Assert.Equal(targetStaffId, result.Items[0].StaffId);
        }
    }
}
