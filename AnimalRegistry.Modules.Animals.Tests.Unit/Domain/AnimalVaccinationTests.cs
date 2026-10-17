using AnimalRegistry.Modules.Animals.Domain.Animals.Vaccinations;
using AnimalRegistry.Shared.DDD;
using FluentAssertions;
using static AnimalRegistry.Modules.Animals.Tests.Unit.VaccinationTestData;

namespace AnimalRegistry.Modules.Animals.Tests.Unit.Domain;

public class AnimalVaccinationTests
{
    private readonly Guid _animalId = Guid.NewGuid();

    private AnimalVaccination Create(AnimalVaccinationData data)
    {
        return AnimalVaccination.Create(_animalId, ShelterId, data, UserEmail, Now);
    }

    [Fact]
    public void Create_ShouldStoreTrimmedData_WhenDataIsValid()
    {
        var validUntil = LocalDate(2027, 3, 10);

        var vaccination = Create(Data("  Nobivac Rabies ", validUntil: validUntil, veterinarian: " Jan Kowalski ",
            batchNumber: " A123 ", notes: " Pierwsza dawka "));

        vaccination.AnimalId.Should().Be(_animalId);
        vaccination.ShelterId.Should().Be(ShelterId);
        vaccination.VaccineName.Should().Be("Nobivac Rabies");
        vaccination.AdministeredOn.Should().Be(AdministeredOn);
        vaccination.ValidUntil.Should().Be(validUntil);
        vaccination.Veterinarian.Should().Be("Jan Kowalski");
        vaccination.BatchNumber.Should().Be("A123");
        vaccination.Notes.Should().Be("Pierwsza dawka");
        vaccination.Attachment.Should().BeNull();
        vaccination.CreatedBy.Should().Be(UserEmail);
        vaccination.CreatedOn.Should().Be(Now);
        vaccination.ModifiedOn.Should().Be(Now);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldStoreNull_WhenOptionalTextIsBlank(string blank)
    {
        var vaccination = Create(Data(veterinarian: blank, batchNumber: blank, notes: blank));

        vaccination.Veterinarian.Should().BeNull();
        vaccination.BatchNumber.Should().BeNull();
        vaccination.Notes.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrow_WhenVaccineNameIsMissing(string name)
    {
        var act = () => Create(Data(name));

        act.Should().Throw<BusinessRuleValidationException>().WithMessage("Vaccine name must be provided.");
    }

    [Fact]
    public void Create_ShouldAcceptAdministrationDate_BeforeStartOfNextDay()
    {
        var act = () => Create(Data(administeredOn: StartOfNextDay.AddTicks(-1)));

        act.Should().NotThrow();
    }

    [Fact]
    public void Create_ShouldThrow_WhenAdministrationDateIsInFuture()
    {
        var act = () => Create(Data(administeredOn: StartOfNextDay));

        act.Should().Throw<BusinessRuleValidationException>()
            .WithMessage("Vaccination date cannot be in the future.");
    }

    [Fact]
    public void Create_ShouldThrow_WhenValidUntilIsBeforeAdministrationDay()
    {
        var act = () => Create(Data(validUntil: LocalDate(2026, 3, 9)));

        act.Should().Throw<BusinessRuleValidationException>()
            .WithMessage("Vaccination validity date cannot be earlier than the vaccination date.");
    }

    [Fact]
    public void Create_ShouldAcceptValidUntil_OnAdministrationDay()
    {
        var act = () => Create(Data(validUntil: LocalDate(2026, 3, 10)));

        act.Should().NotThrow();
    }

    [Fact]
    public void Update_ShouldReplaceDataAndKeepAuthor()
    {
        var vaccination = Create(Data());
        var later = Now.AddHours(1);

        vaccination.Update(Data("Biocan DHPPi", validUntil: LocalDate(2027, 1, 1), veterinarian: null,
            batchNumber: null, notes: "Po korekcie"), later);

        vaccination.VaccineName.Should().Be("Biocan DHPPi");
        vaccination.ValidUntil.Should().Be(LocalDate(2027, 1, 1));
        vaccination.Veterinarian.Should().BeNull();
        vaccination.BatchNumber.Should().BeNull();
        vaccination.Notes.Should().Be("Po korekcie");
        vaccination.CreatedBy.Should().Be(UserEmail);
        vaccination.CreatedOn.Should().Be(Now);
        vaccination.ModifiedOn.Should().Be(later);
    }

    [Fact]
    public void Update_ShouldThrow_WhenDataBreaksRules()
    {
        var vaccination = Create(Data());

        var act = () => vaccination.Update(Data(" "), Now);

        act.Should().Throw<BusinessRuleValidationException>();
        vaccination.VaccineName.Should().Be("Nobivac Rabies");
    }

    [Fact]
    public void SetAttachment_ShouldReplaceAttachment_AndRemoveAttachmentShouldClearIt()
    {
        var vaccination = Create(Data());
        var attachment = VaccinationAttachment.Create("shelter/animal/file.pdf", "file.pdf", "application/pdf", Now);

        vaccination.SetAttachment(attachment, Now);
        vaccination.Attachment.Should().BeSameAs(attachment);

        vaccination.RemoveAttachment(Now);
        vaccination.Attachment.Should().BeNull();
    }

    [Fact]
    public void IsActive_ShouldBeTrue_WhenThereIsNoValidityDate()
    {
        var vaccination = Create(Data());

        vaccination.IsActive(Now).Should().BeTrue();
        vaccination.DaysUntilExpiry(Now).Should().BeNull();
    }

    [Theory]
    [InlineData(2026, 3, 14, false, -1)]
    [InlineData(2026, 3, 15, true, 0)]
    [InlineData(2026, 3, 16, true, 1)]
    [InlineData(2026, 4, 14, true, 30)]
    public void IsActive_ShouldCompareShelterDays(int year, int month, int day, bool expectedActive,
        int expectedDays)
    {
        var vaccination = Create(Data(validUntil: LocalDate(year, month, day)));

        vaccination.IsActive(Now).Should().Be(expectedActive);
        vaccination.DaysUntilExpiry(Now).Should().Be(expectedDays);
    }

    [Fact]
    public void IsActive_ShouldUseShelterDay_WhenUtcDayDiffers()
    {
        var vaccination = Create(Data(administeredOn: LocalDate(2026, 3, 1),
            validUntil: new DateTimeOffset(2026, 3, 14, 23, 30, 0, TimeSpan.Zero)));

        vaccination.IsActive(Now).Should().BeTrue();
        vaccination.DaysUntilExpiry(Now).Should().Be(0);
    }
}
