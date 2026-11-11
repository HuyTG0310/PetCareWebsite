using MockQueryable;
using MockQueryable.Moq;
using Moq;
using PetCareBooking.Application.Features.Pets.Queries.GetPetById;
using PetCareBooking.Application.Features.Pets.Queries.GetPetsWithPagination;
using PetCareBooking.Application.Features.Pets.Queries.SearchPets;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;
using PetCareBooking.Domain.Enums;
using Xunit;

namespace PetCareBooking.UnitTests.Features.Pets.Queries
{
    public class PetQueryHandlersTests
    {
        private readonly Mock<IGenericRepository<Pet>> _petRepoMock = new();
        private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();

        public PetQueryHandlersTests()
        {
            _currentUserServiceMock.Setup(s => s.IsAdminOrStaff).Returns(true);
        }

        [Fact]
        public async Task GetPetsWithPagination_ReturnsCorrectPagedResult()
        {
            // Arrange
            var customer = new Customer { Id = Guid.NewGuid(), FullName = "Owner 1" };
            var petList = new List<Pet>
            {
                new() { Id = Guid.NewGuid(), CustomerId = customer.Id, Name = "Pet 1", Species = PetSpecies.Dog, Weight = 5m, IsActive = true, CreatedAt = DateTime.UtcNow.AddMinutes(-10) },
                new() { Id = Guid.NewGuid(), CustomerId = customer.Id, Name = "Pet 2", Species = PetSpecies.Cat, Weight = 4m, IsActive = true, CreatedAt = DateTime.UtcNow.AddMinutes(-5) },
                new() { Id = Guid.NewGuid(), CustomerId = customer.Id, Name = "Pet 3", Species = PetSpecies.Dog, Weight = 8m, IsActive = false, CreatedAt = DateTime.UtcNow }
            };

            var mockDbSet = petList.BuildMock();
            _petRepoMock.Setup(r => r.GetQueryable()).Returns(mockDbSet);

            var handler = new GetPetsWithPaginationQueryHandler(_petRepoMock.Object);
            var query = new GetPetsWithPaginationQuery
            {
                PageIndex = 1,
                PageSize = 2
            };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.TotalCount);
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(2, result.PageSize);
            Assert.Equal(2, result.Items.Count);
        }

        [Fact]
        public async Task GetPetsWithPagination_WithIsActiveFilter_FiltersCorrectly()
        {
            // Arrange
            var petList = new List<Pet>
            {
                new() { Id = Guid.NewGuid(), Name = "Pet 1", Species = PetSpecies.Dog, Weight = 5m, IsActive = true, CreatedAt = DateTime.UtcNow.AddMinutes(-10) },
                new() { Id = Guid.NewGuid(), Name = "Pet 2", Species = PetSpecies.Cat, Weight = 4m, IsActive = false, CreatedAt = DateTime.UtcNow.AddMinutes(-5) }
            };

            var mockDbSet = petList.BuildMock();
            _petRepoMock.Setup(r => r.GetQueryable()).Returns(mockDbSet);

            var handler = new GetPetsWithPaginationQueryHandler(_petRepoMock.Object);
            var query = new GetPetsWithPaginationQuery
            {
                PageIndex = 1,
                PageSize = 10,
                IsActive = true
            };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(1, result.TotalCount);
            Assert.Single(result.Items);
            Assert.Equal("Pet 1", result.Items[0].Name);
        }

        [Fact]
        public async Task GetPetById_WhenPetDoesNotExist_Returns404NotFound()
        {
            // Arrange
            var emptyList = new List<Pet>();
            var mockDbSet = emptyList.BuildMock();
            _petRepoMock.Setup(r => r.GetQueryable()).Returns(mockDbSet);

            var handler = new GetPetByIdQueryHandler(_petRepoMock.Object, _currentUserServiceMock.Object);
            var query = new GetPetByIdQuery(Guid.NewGuid());

            // Act
            var response = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(response.IsSuccess);
            Assert.Equal(404, response.StatusCode);
            Assert.Null(response.Result);
        }

        [Fact]
        public async Task GetPetById_WhenPetExists_Returns200OKWithDetails()
        {
            // Arrange
            var petId = Guid.NewGuid();
            var customer = new Customer { Id = Guid.NewGuid(), FullName = "Nguyen Van Test" };
            var pet = new Pet
            {
                Id = petId,
                CustomerId = customer.Id,
                Owner = customer,
                Name = "Lucky",
                Species = PetSpecies.Dog,
                Breed = "Beagle",
                Weight = 10.5m,
                Age = 3,
                HealthNotes = "Good",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var petList = new List<Pet> { pet };
            var mockDbSet = petList.BuildMock();
            _petRepoMock.Setup(r => r.GetQueryable()).Returns(mockDbSet);

            var handler = new GetPetByIdQueryHandler(_petRepoMock.Object, _currentUserServiceMock.Object);
            var query = new GetPetByIdQuery(petId);

            // Act
            var response = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(response.IsSuccess);
            Assert.Equal(200, response.StatusCode);
            Assert.NotNull(response.Result);
            Assert.Equal(petId, response.Result.Id);
            Assert.Equal("Lucky", response.Result.Name);
            Assert.Equal("Nguyen Van Test", response.Result.OwnerName);
            Assert.Equal(PetSpecies.Dog, response.Result.Species);
            Assert.Equal(10.5m, response.Result.Weight);
        }

        [Fact]
        public async Task SearchPets_ByKeyword_ReturnsMatchingPets()
        {
            // Arrange
            var petList = new List<Pet>
            {
                new() { Id = Guid.NewGuid(), Name = "Golden Boy", Species = PetSpecies.Dog, Weight = 15m, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), Name = "Silver Shadow", Species = PetSpecies.Cat, Weight = 4m, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), Name = "Boy Junior", Species = PetSpecies.Dog, Weight = 8m, IsActive = true, CreatedAt = DateTime.UtcNow }
            };

            var mockDbSet = petList.BuildMock();
            _petRepoMock.Setup(r => r.GetQueryable()).Returns(mockDbSet);

            var handler = new SearchPetsQueryHandler(_petRepoMock.Object);
            var query = new SearchPetsQuery
            {
                Keyword = "Boy",
                PageIndex = 1,
                PageSize = 10
            };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.TotalCount);
            Assert.All(result.Items, item => Assert.Contains("Boy", item.Name));
        }

        [Fact]
        public async Task SearchPets_BySpecies_ReturnsFilteredPets()
        {
            // Arrange
            var petList = new List<Pet>
            {
                new() { Id = Guid.NewGuid(), Name = "Dog 1", Species = PetSpecies.Dog, Weight = 10m, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), Name = "Cat 1", Species = PetSpecies.Cat, Weight = 3m, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), Name = "Cat 2", Species = PetSpecies.Cat, Weight = 4m, IsActive = true, CreatedAt = DateTime.UtcNow }
            };

            var mockDbSet = petList.BuildMock();
            _petRepoMock.Setup(r => r.GetQueryable()).Returns(mockDbSet);

            var handler = new SearchPetsQueryHandler(_petRepoMock.Object);
            var query = new SearchPetsQuery
            {
                Species = PetSpecies.Cat,
                PageIndex = 1,
                PageSize = 10
            };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.TotalCount);
            Assert.All(result.Items, item => Assert.Equal(PetSpecies.Cat, item.Species));
        }
    }
}
