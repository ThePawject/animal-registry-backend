using AnimalRegistry.Modules.Animals.Api.Reports;
using AnimalRegistry.Modules.Animals.Application.Reports;
using AnimalRegistry.Modules.Animals.Application.Reports.Models;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Tests.Functional.Fixture;
using AnimalRegistry.Shared.Testing;
using FluentAssertions;
using System.Net;

namespace AnimalRegistry.Modules.Animals.Tests.Functional.Origin;

[Collection("Sequential")]
public sealed class OriginReportsTests(ApiTestFixture fixture) : OriginModuleTestBase(fixture)
{
    private static readonly DateTimeOffset March = new(2026, 3, 10, 12, 0, 0, TimeSpan.Zero);

    private static string MonthlyRoute(int year, int month)
    {
        return $"{GenerateOriginMonthlyReportRequest.Route}?year={year}&month={month}";
    }

    private static string YearlyRoute(int year)
    {
        return $"{GenerateOriginYearlyReportRequest.Route}?year={year}";
    }

    private static async Task ShouldBePdfAsync(HttpResponseMessage response, string fileName)
    {
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");
        response.Content.Headers.GetValues("Content-Disposition").Single().Should().Contain(fileName);
        (await response.Content.ReadAsByteArrayAsync()).Take(4).Should().Equal(0x25, 0x50, 0x44, 0x46);
    }

    private Task<OriginMonthlyReportData> MonthlyDataAsync(int year, int month)
    {
        return AsShelterUserAsync<IOriginReportDataService, OriginMonthlyReportData>(service =>
            service.PrepareMonthlyReportDataAsync(ShelterId, year, month));
    }

    private Task<OriginYearlyReportData> YearlyDataAsync(int year)
    {
        return AsShelterUserAsync<IOriginReportDataService, OriginYearlyReportData>(service =>
            service.PrepareYearlyReportDataAsync(ShelterId, year));
    }

    [Fact]
    public async Task Reports_ShouldReturnForbidden_WhenModuleIsDisabled()
    {
        var monthly = await Admin.GetAsync(MonthlyRoute(2026, 3));
        var yearly = await Admin.GetAsync(YearlyRoute(2026));

        monthly.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        yearly.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Theory]
    [InlineData(2026, 0)]
    [InlineData(2026, 13)]
    [InlineData(1999, 3)]
    [InlineData(2101, 3)]
    public async Task MonthlyReport_ShouldReturnBadRequest_WhenPeriodIsInvalid(int year, int month)
    {
        await SetModuleAsync(true);

        var response = await Admin.GetAsync(MonthlyRoute(year, month));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(1999)]
    [InlineData(2101)]
    public async Task YearlyReport_ShouldReturnBadRequest_WhenYearIsInvalid(int year)
    {
        await SetModuleAsync(true);

        var response = await Admin.GetAsync(YearlyRoute(year));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Reports_ShouldReturnPdf_ForShelterWorker()
    {
        await CreateAnimalAsync("2026/0001");
        await SetModuleAsync(true);
        var municipality = await CreateMunicipalityAsync("Gmina A");
        await CreateAnimalAsync("2026/0002", OriginFields(municipality.Id, municipality.Id));

        var monthly = await Worker.GetAsync(MonthlyRoute(2026, 3));
        var yearly = await Worker.GetAsync(YearlyRoute(2026));

        await ShouldBePdfAsync(monthly, "RaportPochodzeniaMiesieczny_03_2026.pdf");
        await ShouldBePdfAsync(yearly, "ZestawienieRocznePochodzenia_2026.pdf");
    }

    [Fact]
    public async Task Reports_ShouldReturnPdf_WhenPeriodHasNoAnimals()
    {
        await SetModuleAsync(true);

        await ShouldBePdfAsync(await Admin.GetAsync(MonthlyRoute(2020, 1)), "RaportPochodzeniaMiesieczny_01_2020.pdf");
        await ShouldBePdfAsync(await Admin.GetAsync(YearlyRoute(2020)), "ZestawienieRocznePochodzenia_2020.pdf");
    }

    [Fact]
    public async Task MonthlyData_ShouldGroupAnimalsAdmittedInMonthByContractor()
    {
        await SetModuleAsync(true);
        var zabki = await CreateMunicipalityAsync("Ząbki");
        var lomianki = await CreateMunicipalityAsync("Łomianki");
        await CreateAnimalAsync("2026/0001", OriginFields(zabki.Id, zabki.Id, admissionDate: March));
        await CreateAnimalAsync("2026/0002", OriginFields(zabki.Id, lomianki.Id, admissionDate: March.AddDays(1)));
        await CreateAnimalAsync("2026/0003", OriginFields(lomianki.Id, zabki.Id, admissionDate: March.AddDays(2)));
        await CreateAnimalAsync("2026/0004", OriginFields(zabki.Id, zabki.Id, admissionDate: March.AddMonths(-1)));

        var data = await MonthlyDataAsync(2026, 3);

        data.Groups.Select(g => g.ContractorName).Should().Equal("Łomianki", "Ząbki");
        data.Groups[1].Entries.Select(e => e.Animal.Signature.Value).Should().Equal("2026/0001", "2026/0003");
        data.Groups[1].Entries[1].Origin.FoundMunicipality.Name.Should().Be("Łomianki");
        data.TotalAnimals.Should().Be(3);
    }

    [Fact]
    public async Task MonthlyData_ShouldUsePolishCalendar()
    {
        await SetModuleAsync(true);
        var municipality = await CreateMunicipalityAsync("Gmina A");
        await CreateAnimalAsync("2026/0001", OriginFields(municipality.Id, municipality.Id,
            admissionDate: new DateTimeOffset(2026, 3, 31, 23, 30, 0, TimeSpan.Zero)));

        var march = await MonthlyDataAsync(2026, 3);
        var april = await MonthlyDataAsync(2026, 4);

        march.TotalAnimals.Should().Be(0);
        april.TotalAnimals.Should().Be(1);
    }

    [Fact]
    public async Task MonthlyData_ShouldListAnimalsWithoutOriginAsUnassigned_InMonthOfRegistration()
    {
        var legacyId = await CreateAnimalAsync("2026/0001");
        await SetModuleAsync(true);
        var municipality = await CreateMunicipalityAsync("Gmina A");
        await CreateAnimalAsync("2026/0002", OriginFields(municipality.Id, municipality.Id,
            admissionDate: DateTimeOffset.Now));
        var now = DateTimeOffset.Now;

        var data = await MonthlyDataAsync(now.Year, now.Month);

        data.Groups.Select(g => g.ContractorName).Should().Equal("Gmina A", "Brak przypisania");
        var unassigned = data.Groups[1];
        unassigned.IsUnassigned.Should().BeTrue();
        unassigned.Entries.Should().ContainSingle().Which.Animal.Id.Should().Be(legacyId);
    }

    [Fact]
    public async Task MonthlyData_ShouldNotIncludeAnimalsOfAnotherShelter()
    {
        var otherAdmin = await CreateOtherShelterAdminWithModuleAsync();
        var foreign = await CreateMunicipalityAsync("Gmina Obca", otherAdmin);
        await new AnimalFactory(new ApiClient(otherAdmin)).CreateAsync("2026/0001", "t-1", "Obcy", "Mix", "none",
            AnimalSpecies.Dog, AnimalSex.Male,
            extraFields: OriginFields(foreign.Id, foreign.Id, admissionDate: March));
        await SetModuleAsync(true);

        var data = await MonthlyDataAsync(2026, 3);

        data.TotalAnimals.Should().Be(0);
    }

    [Fact]
    public async Task YearlyData_ShouldCountAnimalsPerContractorAndMonth()
    {
        await SetModuleAsync(true);
        var zabki = await CreateMunicipalityAsync("Ząbki");
        var lomianki = await CreateMunicipalityAsync("Łomianki");
        await CreateAnimalAsync("2026/0001", OriginFields(zabki.Id, zabki.Id,
            admissionDate: new DateTimeOffset(2026, 1, 15, 12, 0, 0, TimeSpan.Zero)));
        await CreateAnimalAsync("2026/0002", OriginFields(zabki.Id, zabki.Id,
            admissionDate: new DateTimeOffset(2026, 1, 31, 23, 30, 0, TimeSpan.Zero)));
        await CreateAnimalAsync("2026/0003", OriginFields(lomianki.Id, lomianki.Id, admissionDate: March));
        await CreateAnimalAsync("2026/0004", OriginFields(lomianki.Id, lomianki.Id,
            admissionDate: new DateTimeOffset(2025, 12, 31, 12, 0, 0, TimeSpan.Zero)));

        var data = await YearlyDataAsync(2026);

        data.Rows.Select(r => r.ContractorName).Should().Equal("Łomianki", "Ząbki");
        data.Rows[0].MonthCounts.Should().Equal(0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        data.Rows[1].MonthCounts.Should().Equal(1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        data.Total.Should().Be(3);
    }

    [Fact]
    public async Task YearlyData_ShouldReflectChangedContractor()
    {
        await SetModuleAsync(true);
        var zabki = await CreateMunicipalityAsync("Ząbki");
        var lomianki = await CreateMunicipalityAsync("Łomianki");
        var id = await CreateAnimalAsync("2026/0001", OriginFields(zabki.Id, zabki.Id, admissionDate: March));

        (await PutAnimalAsync(id, "2026/0001", OriginFields(zabki.Id, lomianki.Id, admissionDate: March)))
            .EnsureSuccessStatusCode();

        var data = await YearlyDataAsync(2026);
        data.Rows.Should().ContainSingle().Which.ContractorName.Should().Be("Łomianki");
    }

    [Fact]
    public async Task AnimalReports_ShouldIncludeOriginRows_WhenModuleIsEnabled()
    {
        await SetModuleAsync(true);
        var municipality = await CreateMunicipalityAsync("Gmina A");
        var id = await CreateAnimalAsync("2026/0001", OriginFields(municipality.Id, municipality.Id));
        var today = DateTimeOffset.UtcNow;

        var responses = new[]
        {
            await Admin.GetAsync("/reports/animals/dump"),
            await Admin.GetAsync($"{GenerateSelectedAnimalsReportRequest.Route}?ids={id}"),
            await Admin.GetAsync(
                $"{GenerateDateRangeAnimalsReportRequest.Route}?startDate={Uri.EscapeDataString(today.AddDays(-1).ToString("o"))}&endDate={Uri.EscapeDataString(today.AddDays(1).ToString("o"))}"),
        };

        responses.Should().OnlyContain(r => r.StatusCode == HttpStatusCode.OK);
    }
}
