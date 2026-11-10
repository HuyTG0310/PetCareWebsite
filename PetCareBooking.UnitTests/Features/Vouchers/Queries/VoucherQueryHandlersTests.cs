using MockQueryable;
using MockQueryable.Moq;
using Moq;
using PetCareBooking.Application.Features.Vouchers.Queries.GetVoucherById;
using PetCareBooking.Application.Features.Vouchers.Queries.GetVouchersWithPagination;
using PetCareBooking.Application.Features.Vouchers.Queries.SearchVouchers;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;
using PetCareBooking.Domain.Enums;
using Xunit;

namespace PetCareBooking.UnitTests.Features.Vouchers.Queries
{
    public class VoucherQueryHandlersTests
    {
        private readonly Mock<IGenericRepository<Voucher>> _voucherRepoMock = new();

        [Fact]
        public async Task GetVouchersWithPagination_ReturnsCorrectPagedResultAndCalculatesIsValid()
        {
            var now = DateTime.UtcNow;
            var vouchers = new List<Voucher>
            {
                // Còn hạn: EndDate tương lai, chưa hết lượt dùng
                new() { Id = Guid.NewGuid(), Code = "ACTIVE1", DiscountType = DiscountType.Percentage, DiscountValue = 10, StartDate = now.AddDays(-1), EndDate = now.AddDays(5), MaxUsage = 10, CurrentUsage = 2, CreatedAt = now.AddMinutes(-10) },
                // Hết hạn: EndDate đã qua
                new() { Id = Guid.NewGuid(), Code = "EXPIRED", DiscountType = DiscountType.Percentage, DiscountValue = 15, StartDate = now.AddDays(-10), EndDate = now.AddDays(-1), MaxUsage = 10, CurrentUsage = 0, CreatedAt = now.AddMinutes(-5) },
                // Hết lượt: CurrentUsage == MaxUsage
                new() { Id = Guid.NewGuid(), Code = "MAXEDOUT", DiscountType = DiscountType.Fixed_Amount, DiscountValue = 50000, StartDate = now.AddDays(-1), EndDate = now.AddDays(5), MaxUsage = 5, CurrentUsage = 5, CreatedAt = now }
            };

            var mockDbSet = vouchers.BuildMock();
            _voucherRepoMock.Setup(r => r.GetQueryable()).Returns(mockDbSet);

            var handler = new GetVouchersWithPaginationQueryHandler(_voucherRepoMock.Object);
            var query = new GetVouchersWithPaginationQuery { PageIndex = 1, PageSize = 10 };

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(3, result.TotalCount);
            Assert.Equal(3, result.Items.Count);

            var activeVoucher = result.Items.First(v => v.Code == "ACTIVE1");
            Assert.True(activeVoucher.IsValid);

            var expiredVoucher = result.Items.First(v => v.Code == "EXPIRED");
            Assert.False(expiredVoucher.IsValid);

            var maxedOutVoucher = result.Items.First(v => v.Code == "MAXEDOUT");
            Assert.False(maxedOutVoucher.IsValid);
        }

        [Fact]
        public async Task GetVoucherById_WhenNotFound_Returns404()
        {
            var emptyList = new List<Voucher>();
            var mockDbSet = emptyList.BuildMock();
            _voucherRepoMock.Setup(r => r.GetQueryable()).Returns(mockDbSet);

            var handler = new GetVoucherByIdQueryHandler(_voucherRepoMock.Object);
            var query = new GetVoucherByIdQuery(Guid.NewGuid());

            var response = await handler.Handle(query, CancellationToken.None);

            Assert.False(response.IsSuccess);
            Assert.Equal(404, response.StatusCode);
            Assert.Null(response.Result);
        }

        [Fact]
        public async Task GetVoucherById_WhenFound_Returns200WithFullDetails()
        {
            var id = Guid.NewGuid();
            var now = DateTime.UtcNow;
            var voucher = new Voucher
            {
                Id = id,
                Code = "VIP100",
                DiscountType = DiscountType.Fixed_Amount,
                DiscountValue = 100000,
                StartDate = now.AddDays(-2),
                EndDate = now.AddDays(10),
                MaxUsage = 20,
                CurrentUsage = 1,
                CreatedAt = now.AddDays(-2)
            };

            var list = new List<Voucher> { voucher };
            var mockDbSet = list.BuildMock();
            _voucherRepoMock.Setup(r => r.GetQueryable()).Returns(mockDbSet);

            var handler = new GetVoucherByIdQueryHandler(_voucherRepoMock.Object);
            var query = new GetVoucherByIdQuery(id);

            var response = await handler.Handle(query, CancellationToken.None);

            Assert.True(response.IsSuccess);
            Assert.Equal(200, response.StatusCode);
            Assert.NotNull(response.Result);
            Assert.Equal("VIP100", response.Result.Code);
            Assert.True(response.Result.IsValid);
            Assert.Equal(100000, response.Result.DiscountValue);
        }

        [Fact]
        public async Task SearchVouchers_ByKeyword_ReturnsMatching()
        {
            var vouchers = new List<Voucher>
            {
                new() { Id = Guid.NewGuid(), Code = "SUMMER2026", EndDate = DateTime.UtcNow.AddDays(10), CreatedAt = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), Code = "WINTER2026", EndDate = DateTime.UtcNow.AddDays(10), CreatedAt = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), Code = "SUMMERFLASH", EndDate = DateTime.UtcNow.AddDays(10), CreatedAt = DateTime.UtcNow }
            };

            var mockDbSet = vouchers.BuildMock();
            _voucherRepoMock.Setup(r => r.GetQueryable()).Returns(mockDbSet);

            var handler = new SearchVouchersQueryHandler(_voucherRepoMock.Object);
            var query = new SearchVouchersQuery { Keyword = "summer", PageIndex = 1, PageSize = 10 };

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.Equal(2, result.TotalCount);
            Assert.All(result.Items, v => Assert.Contains("SUMMER", v.Code));
        }

        [Fact]
        public async Task SearchVouchers_WithIsValidOnly_FiltersOnlyValidVouchers()
        {
            var now = DateTime.UtcNow;
            var vouchers = new List<Voucher>
            {
                // Hợp lệ
                new() { Id = Guid.NewGuid(), Code = "VALID1", EndDate = now.AddDays(5), MaxUsage = 10, CurrentUsage = 2, CreatedAt = now },
                // Hết hạn
                new() { Id = Guid.NewGuid(), Code = "EXPIRED1", EndDate = now.AddDays(-2), MaxUsage = 10, CurrentUsage = 2, CreatedAt = now },
                // Hết lượt
                new() { Id = Guid.NewGuid(), Code = "EXHAUSTED", EndDate = now.AddDays(5), MaxUsage = 2, CurrentUsage = 2, CreatedAt = now }
            };

            var mockDbSet = vouchers.BuildMock();
            _voucherRepoMock.Setup(r => r.GetQueryable()).Returns(mockDbSet);

            var handler = new SearchVouchersQueryHandler(_voucherRepoMock.Object);
            var query = new SearchVouchersQuery { IsValidOnly = true, PageIndex = 1, PageSize = 10 };

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.Equal(1, result.TotalCount);
            Assert.Single(result.Items);
            Assert.Equal("VALID1", result.Items[0].Code);
            Assert.True(result.Items[0].IsValid);
        }
    }
}
