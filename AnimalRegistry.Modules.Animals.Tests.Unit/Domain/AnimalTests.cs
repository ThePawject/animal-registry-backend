using System;
using FluentAssertions;
using Xunit;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.DomainEvents;

namespace AnimalRegistry.Modules.Animals.Tests.Unit.Domain
{
    public class AnimalTests
    {
        private Animal CreateTestAnimal(
            string signature = "SIG123",
            string transponderCode = "TR123",
            string name = "Burek",
            string color = "Brown",
            AnimalSpecies species = AnimalSpecies.Dog,
            AnimalSex sex = AnimalSex.Male,
            DateTimeOffset? birthDate = null)
        {
            return Animal.Create(
                signature,
                transponderCode,
                name,
                color,
                species,
                sex,
                birthDate ?? new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero)
            );
        }

        [Fact]
        public void Create_ShouldReturnAnimal_WithCorrectProperties_AndRaiseCreatedEvent()
        {
            // Arrange
            var signature = "SIG123";
            var transponderCode = "TR123";
            var name = "Burek";
            var color = "Brown";
            var species = AnimalSpecies.Dog;
            var sex = AnimalSex.Male;
            var birthDate = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);

            // Act
            var animal = CreateTestAnimal(signature, transponderCode, name, color, species, sex, birthDate);

            // Assert
            animal.Should().NotBeNull();
            animal.Signature.Should().Be(signature, "signature should be set");
            animal.TransponderCode.Should().Be(transponderCode, "transponderCode should be set");
            animal.Name.Should().Be(name, "name should be set");
            animal.Color.Should().Be(color, "color should be set");
            animal.Species.Should().Be(species, "species should be set");
            animal.Sex.Should().Be(sex, "sex should be set");
            animal.BirthDate.Should().Be(birthDate, "birthDate should be set");
            animal.IsActive.Should().BeTrue("animal should be active after creation");
            animal.CreatedOn.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromSeconds(10));
            animal.ModifiedOn.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(10));

            animal.DomainEvents.Should().ContainSingle(e => e is AnimalCreatedDomainEvent, "should raise exactly one created event");
            var createdEvent = animal.DomainEvents.Single(e => e is AnimalCreatedDomainEvent) as AnimalCreatedDomainEvent;
            createdEvent.Should().NotBeNull();
            createdEvent!.AnimalId.Should().Be(animal.Id);
            createdEvent.Signature.Should().Be(signature);
            createdEvent.Name.Should().Be(name);
        }

        [Fact]
        public void Archive_ShouldSetIsActiveFalse_AndRaiseArchivedEvent()
        {
            // Arrange
            var animal = CreateTestAnimal();
            animal.ClearDomainEvents();

            // Act
            animal.Archive();

            // Assert
            animal.IsActive.Should().BeFalse("animal should be inactive after archiving");
            animal.ModifiedOn.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(10));
            animal.DomainEvents.Should().ContainSingle(e => e is AnimalArchivedDomainEvent, "should raise exactly one archived event");
            var archivedEvent = animal.DomainEvents.Single(e => e is AnimalArchivedDomainEvent) as AnimalArchivedDomainEvent;
            archivedEvent.Should().NotBeNull();
            archivedEvent!.AnimalId.Should().Be(animal.Id);
        }

        [Fact]
        public void Archive_ShouldNotRaiseEvent_WhenAlreadyArchived()
        {
            // Arrange
            var animal = CreateTestAnimal();
            animal.Archive();
            animal.ClearDomainEvents();

            // Act
            animal.Archive();

            // Assert
            animal.IsActive.Should().BeFalse("animal should remain inactive after repeated archiving");
            animal.DomainEvents.Should().BeEmpty("no event should be raised when archiving already archived animal");
        }

        [Fact]
        public void Create_ShouldAllowEmptyStrings_AndRaiseEvent()
        {
            // Arrange
            var animal = CreateTestAnimal("", "", "", "", AnimalSpecies.None, AnimalSex.None);

            // Assert
            animal.Signature.Should().BeEmpty();
            animal.TransponderCode.Should().BeEmpty();
            animal.Name.Should().BeEmpty();
            animal.Color.Should().BeEmpty();
            animal.Species.Should().Be(AnimalSpecies.None);
            animal.Sex.Should().Be(AnimalSex.None);
            animal.DomainEvents.Should().ContainSingle(e => e is AnimalCreatedDomainEvent);
        }

        [Fact]
        public void Create_ShouldAllowFutureBirthDate()
        {
            // Arrange
            var futureDate = DateTimeOffset.UtcNow.AddYears(5);
            var animal = CreateTestAnimal(birthDate: futureDate);

            // Assert
            animal.BirthDate.Should().Be(futureDate);
        }
    }
}
