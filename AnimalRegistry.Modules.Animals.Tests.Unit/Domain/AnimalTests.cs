using System;
using FluentAssertions;
using Xunit;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.DomainEvents;

namespace AnimalRegistry.Modules.Animals.Tests.Unit.Domain
{
    public class AnimalTests
    {
        private const string TestShelterId = "test-shelter-id";

        private static Animal CreateTestAnimal(
            string signature = "SIG123",
            string transponderCode = "TR123",
            string name = "Burek",
            string color = "Brown",
            AnimalSpecies species = AnimalSpecies.Dog,
            AnimalSex sex = AnimalSex.Male,
            DateTimeOffset? birthDate = null,
            string? shelterId = null)
        {
            return Animal.Create(
                signature,
                transponderCode,
                name,
                color,
                species,
                sex,
                birthDate ?? new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
                shelterId ?? TestShelterId
            );
        }

        [Fact]
        public void Create_ShouldReturnAnimal_WithCorrectProperties_AndRaiseCreatedEvent()
        {
            const string signature = "SIG123";
            const string transponderCode = "TR123";
            const string name = "Burek";
            const string color = "Brown";
            const AnimalSpecies species = AnimalSpecies.Dog;
            const AnimalSex sex = AnimalSex.Male;
            var birthDate = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);

            var animal = CreateTestAnimal(signature, transponderCode, name, color, species, sex, birthDate);

            animal.Should().NotBeNull();
            animal.Signature.Should().Be(signature);
            animal.TransponderCode.Should().Be(transponderCode);
            animal.Name.Should().Be(name);
            animal.Color.Should().Be(color);
            animal.Species.Should().Be(species);
            animal.Sex.Should().Be(sex);
            animal.BirthDate.Should().Be(birthDate);
            animal.IsActive.Should().BeTrue();
            animal.CreatedOn.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromSeconds(10));
            animal.ModifiedOn.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(10));
            animal.ShelterId.Should().Be(TestShelterId);

            animal.DomainEvents.Should().ContainSingle(e => e is AnimalCreatedDomainEvent);
            var createdEvent = animal.DomainEvents.Single(e => e is AnimalCreatedDomainEvent) as AnimalCreatedDomainEvent;
            createdEvent.Should().NotBeNull();
            createdEvent.AnimalId.Should().Be(animal.Id);
            createdEvent.Signature.Should().Be(signature);
            createdEvent.Name.Should().Be(name);
        }

        [Fact]
        public void Archive_ShouldSetIsActiveFalse_AndRaiseArchivedEvent()
        {
            var animal = CreateTestAnimal();
            animal.ClearDomainEvents();

            animal.Archive();

            animal.IsActive.Should().BeFalse();
            animal.ModifiedOn.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(10));
            animal.DomainEvents.Should().ContainSingle(e => e is AnimalArchivedDomainEvent);
            var archivedEvent = animal.DomainEvents.Single(e => e is AnimalArchivedDomainEvent) as AnimalArchivedDomainEvent;
            archivedEvent.Should().NotBeNull();
            archivedEvent!.AnimalId.Should().Be(animal.Id);
        }

        [Fact]
        public void Archive_ShouldNotRaiseEvent_WhenAlreadyArchived()
        {
            var animal = CreateTestAnimal();
            animal.Archive();
            animal.ClearDomainEvents();

            animal.Archive();

            animal.IsActive.Should().BeFalse();
            animal.DomainEvents.Should().BeEmpty();
        }

        [Fact]
        public void Create_ShouldAllowEmptyStrings_AndRaiseEvent()
        {
            var animal = CreateTestAnimal("", "", "", "", AnimalSpecies.None, AnimalSex.None);

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
            var futureDate = DateTimeOffset.UtcNow.AddYears(5);
            var animal = CreateTestAnimal(birthDate: futureDate);

            animal.BirthDate.Should().Be(futureDate);
        }
    }
}
