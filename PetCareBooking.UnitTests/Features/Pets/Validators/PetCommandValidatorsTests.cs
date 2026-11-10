using FluentValidation.TestHelper;
using PetCareBooking.Application.Features.Pets.Commands.CreatePet;
using PetCareBooking.Application.Features.Pets.Commands.DeletePet;
using PetCareBooking.Application.Features.Pets.Commands.UpdatePet;
using PetCareBooking.Domain.Enums;
using Xunit;

namespace PetCareBooking.UnitTests.Features.Pets.Validators
{
    public class PetCommandValidatorsTests
    {
        private readonly CreatePetCommandValidator _createValidator = new();
        private readonly UpdatePetCommandValidator _updateValidator = new();
        private readonly DeletePetCommandValidator _deleteValidator = new();

        [Fact]
        public void CreatePetValidator_ValidCommand_ShouldNotHaveAnyErrors()
        {
            // Arrange
            var command = new CreatePetCommand
            {
                CustomerId = Guid.NewGuid(),
                Name = "Milo",
                Species = PetSpecies.Dog,
                Breed = "Golden Retriever",
                Weight = 12.5m,
                Age = 2,
                HealthNotes = "Healthy"
            };

            // Act
            var result = _createValidator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void CreatePetValidator_EmptyCustomerId_ShouldHaveError()
        {
            var command = new CreatePetCommand
            {
                CustomerId = Guid.Empty,
                Name = "Milo",
                Species = PetSpecies.Dog,
                Weight = 5.0m
            };

            var result = _createValidator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.CustomerId)
                  .WithErrorMessage("CustomerId is required.");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void CreatePetValidator_EmptyName_ShouldHaveError(string? invalidName)
        {
            var command = new CreatePetCommand
            {
                CustomerId = Guid.NewGuid(),
                Name = invalidName!,
                Species = PetSpecies.Dog,
                Weight = 5.0m
            };

            var result = _createValidator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void CreatePetValidator_NameExceeds100Chars_ShouldHaveError()
        {
            var command = new CreatePetCommand
            {
                CustomerId = Guid.NewGuid(),
                Name = new string('A', 101),
                Species = PetSpecies.Cat,
                Weight = 3.5m
            };

            var result = _createValidator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Name)
                  .WithErrorMessage("Pet name cannot exceed 100 characters.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1.5)]
        public void CreatePetValidator_ZeroOrNegativeWeight_ShouldHaveError(decimal invalidWeight)
        {
            var command = new CreatePetCommand
            {
                CustomerId = Guid.NewGuid(),
                Name = "Luna",
                Species = PetSpecies.Cat,
                Weight = invalidWeight
            };

            var result = _createValidator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Weight)
                  .WithErrorMessage("Weight must be greater than 0.");
        }

        [Fact]
        public void CreatePetValidator_NegativeAge_ShouldHaveError()
        {
            var command = new CreatePetCommand
            {
                CustomerId = Guid.NewGuid(),
                Name = "Luna",
                Species = PetSpecies.Cat,
                Weight = 4.0m,
                Age = -1
            };

            var result = _createValidator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Age)
                  .WithErrorMessage("Age must be greater than or equal to 0.");
        }

        [Fact]
        public void UpdatePetValidator_EmptyId_ShouldHaveError()
        {
            var command = new UpdatePetCommand
            {
                Id = Guid.Empty,
                Name = "Milo Updated",
                Species = PetSpecies.Dog,
                Weight = 14.0m
            };

            var result = _updateValidator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Pet ID is required.");
        }

        [Fact]
        public void UpdatePetValidator_ValidCommand_ShouldNotHaveAnyErrors()
        {
            var command = new UpdatePetCommand
            {
                Id = Guid.NewGuid(),
                Name = "Milo Updated",
                Species = PetSpecies.Dog,
                Breed = "Poodle",
                Weight = 8.5m,
                Age = 3,
                HealthNotes = "Fully vaccinated"
            };

            var result = _updateValidator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void DeletePetValidator_EmptyId_ShouldHaveError()
        {
            var command = new DeletePetCommand(Guid.Empty);

            var result = _deleteValidator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Pet ID is required.");
        }

        [Fact]
        public void DeletePetValidator_ValidId_ShouldNotHaveErrors()
        {
            var command = new DeletePetCommand(Guid.NewGuid());

            var result = _deleteValidator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
