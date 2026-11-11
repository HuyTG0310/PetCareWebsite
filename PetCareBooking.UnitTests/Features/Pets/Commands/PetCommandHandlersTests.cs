using Moq;
using PetCareBooking.Application.Features.Pets.Commands.CreatePet;
using PetCareBooking.Application.Features.Pets.Commands.DeletePet;
using PetCareBooking.Application.Features.Pets.Commands.UpdatePet;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;
using PetCareBooking.Domain.Enums;
using Xunit;

namespace PetCareBooking.UnitTests.Features.Pets.Commands
{
    public class PetCommandHandlersTests
    {
        private readonly Mock<IGenericRepository<Pet>> _petRepoMock = new();
        private readonly Mock<IGenericRepository<Customer>> _customerRepoMock = new();
        private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

        public PetCommandHandlersTests()
        {
            _currentUserServiceMock.Setup(s => s.IsAdminOrStaff).Returns(true);
        }

        [Fact]
        public async Task CreatePet_WhenCustomerDoesNotExist_Returns404NotFound()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            _customerRepoMock.Setup(r => r.GetByIdAsync(customerId))
                             .ReturnsAsync((Customer?)null);

            var handler = new CreatePetCommandHandler(
                _petRepoMock.Object,
                _customerRepoMock.Object,
                _currentUserServiceMock.Object,
                _unitOfWorkMock.Object);

            var command = new CreatePetCommand
            {
                CustomerId = customerId,
                Name = "Buddy",
                Species = PetSpecies.Dog,
                Weight = 10m
            };

            // Act
            var response = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(response.IsSuccess);
            Assert.Equal(404, response.StatusCode);
            Assert.Equal("Customer not found.", response.Message);
            _petRepoMock.Verify(r => r.AddAsync(It.IsAny<Pet>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreatePet_WhenCustomerExists_Returns201CreatedAndSetsIsActiveTrue()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var customer = new Customer { Id = customerId, FullName = "Nguyen Van A" };
            _customerRepoMock.Setup(r => r.GetByIdAsync(customerId))
                             .ReturnsAsync(customer);

            Pet? savedPet = null;
            _petRepoMock.Setup(r => r.AddAsync(It.IsAny<Pet>()))
                        .Callback<Pet>(p => savedPet = p)
                        .Returns(Task.CompletedTask);

            var handler = new CreatePetCommandHandler(
                _petRepoMock.Object,
                _customerRepoMock.Object,
                _currentUserServiceMock.Object,
                _unitOfWorkMock.Object);

            var command = new CreatePetCommand
            {
                CustomerId = customerId,
                Name = "Buddy",
                Species = PetSpecies.Dog,
                Breed = "Corgi",
                Weight = 11.2m,
                Age = 2,
                HealthNotes = "None"
            };

            // Act
            var response = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(response.IsSuccess);
            Assert.Equal(201, response.StatusCode);
            Assert.NotEqual(Guid.Empty, response.Result);
            Assert.NotNull(savedPet);
            Assert.Equal("Buddy", savedPet.Name);
            Assert.True(savedPet.IsActive);
            Assert.Equal(customerId, savedPet.CustomerId);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdatePet_WhenPetDoesNotExist_Returns404NotFound()
        {
            // Arrange
            var petId = Guid.NewGuid();
            _petRepoMock.Setup(r => r.GetByIdAsync(petId))
                        .ReturnsAsync((Pet?)null);

            var handler = new UpdatePetCommandHandler(
                _petRepoMock.Object,
                _currentUserServiceMock.Object,
                _unitOfWorkMock.Object);

            var command = new UpdatePetCommand
            {
                Id = petId,
                Name = "Updated Name",
                Species = PetSpecies.Cat,
                Weight = 4m
            };

            // Act
            var response = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(response.IsSuccess);
            Assert.Equal(404, response.StatusCode);
            _petRepoMock.Verify(r => r.Update(It.IsAny<Pet>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdatePet_WhenPetExists_UpdatesFieldsAndReturns200OK()
        {
            // Arrange
            var petId = Guid.NewGuid();
            var existingPet = new Pet
            {
                Id = petId,
                Name = "Old Name",
                Species = PetSpecies.Dog,
                Weight = 10m,
                IsActive = true
            };

            _petRepoMock.Setup(r => r.GetByIdAsync(petId))
                        .ReturnsAsync(existingPet);

            var handler = new UpdatePetCommandHandler(
                _petRepoMock.Object,
                _currentUserServiceMock.Object,
                _unitOfWorkMock.Object);

            var command = new UpdatePetCommand
            {
                Id = petId,
                Name = "New Name",
                Species = PetSpecies.Dog,
                Breed = "Husky",
                Weight = 20m,
                Age = 4,
                HealthNotes = "Recovered"
            };

            // Act
            var response = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(response.IsSuccess);
            Assert.Equal(200, response.StatusCode);
            Assert.Equal(petId, response.Result);
            Assert.Equal("New Name", existingPet.Name);
            Assert.Equal("Husky", existingPet.Breed);
            Assert.Equal(20m, existingPet.Weight);
            Assert.Equal(4, existingPet.Age);
            Assert.NotNull(existingPet.UpdatedAt);
            _petRepoMock.Verify(r => r.Update(existingPet), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeletePet_WhenPetDoesNotExist_Returns404NotFound()
        {
            // Arrange
            var petId = Guid.NewGuid();
            _petRepoMock.Setup(r => r.GetByIdAsync(petId))
                        .ReturnsAsync((Pet?)null);

            var handler = new DeletePetCommandHandler(
                _petRepoMock.Object,
                _currentUserServiceMock.Object,
                _unitOfWorkMock.Object);

            var command = new DeletePetCommand(petId);

            // Act
            var response = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(response.IsSuccess);
            Assert.Equal(404, response.StatusCode);
            Assert.False(response.Result);
            _petRepoMock.Verify(r => r.Update(It.IsAny<Pet>()), Times.Never);
        }

        [Fact]
        public async Task DeletePet_WhenPetAlreadyInactive_Returns400BadRequest()
        {
            // Arrange
            var petId = Guid.NewGuid();
            var inactivePet = new Pet
            {
                Id = petId,
                Name = "Ghost",
                IsActive = false
            };

            _petRepoMock.Setup(r => r.GetByIdAsync(petId))
                        .ReturnsAsync(inactivePet);

            var handler = new DeletePetCommandHandler(
                _petRepoMock.Object,
                _currentUserServiceMock.Object,
                _unitOfWorkMock.Object);

            var command = new DeletePetCommand(petId);

            // Act
            var response = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(response.IsSuccess);
            Assert.Equal(400, response.StatusCode);
            Assert.Equal("This pet is already deleted (inactive).", response.Message);
            _petRepoMock.Verify(r => r.Update(It.IsAny<Pet>()), Times.Never);
        }

        [Fact]
        public async Task DeletePet_WhenPetIsActive_SetsIsActiveFalseAndReturns200OK()
        {
            // Arrange
            var petId = Guid.NewGuid();
            var activePet = new Pet
            {
                Id = petId,
                Name = "Active Pet",
                IsActive = true
            };

            _petRepoMock.Setup(r => r.GetByIdAsync(petId))
                        .ReturnsAsync(activePet);

            var handler = new DeletePetCommandHandler(
                _petRepoMock.Object,
                _currentUserServiceMock.Object,
                _unitOfWorkMock.Object);

            var command = new DeletePetCommand(petId);

            // Act
            var response = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(response.IsSuccess);
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.Result);
            Assert.False(activePet.IsActive);
            Assert.NotNull(activePet.UpdatedAt);
            _petRepoMock.Verify(r => r.Update(activePet), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
