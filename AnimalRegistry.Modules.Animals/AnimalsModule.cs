using AnimalRegistry.Modules.Animals.Api;
using AnimalRegistry.Modules.Animals.Application;
using AnimalRegistry.Modules.Animals.Application.Origin;
using AnimalRegistry.Modules.Animals.Application.Reports;
using AnimalRegistry.Modules.Animals.Application.Vaccinations;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Domain.Animals.AnimalEvents;
using AnimalRegistry.Modules.Animals.Domain.Animals.Origin;
using AnimalRegistry.Modules.Animals.Domain.Animals.Vaccinations;
using AnimalRegistry.Modules.Animals.Domain.Municipalities;
using AnimalRegistry.Modules.Animals.Infrastructure;
using AnimalRegistry.Modules.Animals.Infrastructure.Animals;
using AnimalRegistry.Modules.Animals.Infrastructure.Municipalities;
using AnimalRegistry.Modules.Animals.Infrastructure.Origin;
using AnimalRegistry.Modules.Animals.Infrastructure.Services;
using AnimalRegistry.Modules.Animals.Infrastructure.Services.Pdf.ReportPdfs;
using AnimalRegistry.Modules.Animals.Infrastructure.Services.ReportData;
using AnimalRegistry.Modules.Animals.Infrastructure.Vaccinations;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;
using AnimalRegistry.Shared.ShelterFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace AnimalRegistry.Modules.Animals;

public sealed class AnimalsModule : IModule
{
    public string Name => "Animals";

    public IReadOnlyCollection<Assembly> EndpointAssemblies => [typeof(CreateAnimal).Assembly];

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediator(typeof(CreateAnimalCommandHandler).Assembly);

        services.Configure<AnimalsDatabaseSettings>(options =>
        {
            options.ConnectionString = configuration["Database:ConnectionString"]!;
        });
        services.AddDbContext<AnimalsDbContext>((serviceProvider, options) =>
        {
            var dbSettings = serviceProvider.GetRequiredService<IOptions<AnimalsDatabaseSettings>>().Value;
            options.UseSqlServer(dbSettings.ConnectionString);
        });

        services.AddScoped<IAnimalRepository, AnimalRepository>();
        services.AddScoped<IAnimalEventRepository, AnimalEventRepository>();
        services.AddScoped<IMunicipalityRepository, MunicipalityRepository>();
        services.AddScoped<IAnimalsUnitOfWork, AnimalsUnitOfWork>();

        services.AddScoped<IRepositoryDumpDataService, RepositoryDumpDataService>();
        services.AddScoped<ISelectedAnimalsDataService, SelectedAnimalsDataService>();
        services.AddScoped<IDateRangeAnimalsDataService, DateRangeAnimalsDataService>();

        services.AddScoped<IAnimalSignatureService, AnimalSignatureService>();
        services.AddScoped<IEventReportPdfService, EventReportPdfService>();
        services.AddScoped<IDateRangeAnimalsReportPdfService, DateRangeAnimalsReportPdfService>();
        services.AddScoped<ISelectedAnimalsReportPdfService, SelectedAnimalsReportPdfService>();
        services.AddScoped<IRepositoryDumpReportPdfService, RepositoryDumpReportPdfService>();
        services.AddScoped<IAnimalReportFieldsProvider, AnimalReportFieldsProvider>();

        RegisterOriginFeature(services);
        RegisterVaccinationFeature(services);

        services.Configure<BlobStorageSettings>(options =>
        {
            options.ConnectionString = configuration["BlobStorage:ConnectionString"];
            options.ContainerName = configuration["BlobStorage:ContainerName"]!;
            options.AccountName = configuration["BlobStorage:AccountName"]!;
        });
        services.AddSingleton<IImageOptimizationService, ImageOptimizationService>();
        services.AddSingleton<IBlobStorageService, BlobStorageService>();
    }

    public async Task MigrateAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AnimalsDbContext>();

        var pending = await db.Database.GetPendingMigrationsAsync();

        if (!pending.Any())
        {
            return;
        }

        await db.Database.MigrateAsync();
    }

    private static void RegisterOriginFeature(IServiceCollection services)
    {
        services.AddShelterFeature<AnimalOriginFeature>();
        services.AddScoped<IAnimalOriginReader, AnimalOriginReader>();
        services.AddScoped<IAnimalOriginWriter, AnimalOriginWriter>();
        services.AddScoped<IAnimalReportFieldContributor, OriginReportFieldContributor>();
        services.AddScoped<IOriginReportDataService, OriginReportDataService>();
        services.AddScoped<IOriginReportQuery, OriginReportQuery>();
        services.AddScoped<IOriginMonthlyReportPdfService, OriginMonthlyReportPdfService>();
        services.AddScoped<IOriginYearlyReportPdfService, OriginYearlyReportPdfService>();
    }

    private static void RegisterVaccinationFeature(IServiceCollection services)
    {
        services.AddShelterFeature<AnimalVaccinationFeature>();
        services.AddScoped<IAnimalVaccinationRepository, AnimalVaccinationRepository>();
        services.AddScoped<IExpiringVaccinationsReportPdfService, ExpiringVaccinationsReportPdfService>();
        services.AddScoped<IVaccinationsReportPdfService, VaccinationsReportPdfService>();
    }
}