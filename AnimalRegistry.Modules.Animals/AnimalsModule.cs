using AnimalRegistry.Modules.Animals.Api;
using AnimalRegistry.Modules.Animals.Application;
using AnimalRegistry.Modules.Animals.Application.Reports;
using AnimalRegistry.Modules.Animals.Domain.Animals;
using AnimalRegistry.Modules.Animals.Infrastructure;
using AnimalRegistry.Modules.Animals.Infrastructure.Animals;
using AnimalRegistry.Modules.Animals.Infrastructure.Services;
using AnimalRegistry.Modules.Animals.Infrastructure.Services.Pdf.ReportPdfs;
using AnimalRegistry.Modules.Animals.Infrastructure.Services.ReportData;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AnimalRegistry.Modules.Animals;

public sealed class AnimalsModule : IModule
{
    public string Name => "Animals";

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddFastEndpoints(options =>
        {
            options.Assemblies =
            [
                typeof(CreateAnimal).Assembly,
            ];
        });

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

        // Repositories
        services.AddScoped<IAnimalRepository, AnimalRepository>();
        services.AddScoped<IAnimalEventRepository, AnimalEventRepository>();

        // Report Data Services
        services.AddScoped<IRepositoryDumpDataService, RepositoryDumpDataService>();
        services.AddScoped<ISelectedAnimalsDataService, SelectedAnimalsDataService>();
        services.AddScoped<IDateRangeAnimalsDataService, DateRangeAnimalsDataService>();

        // Report PDF Services
        services.AddScoped<IEventReportPdfService, EventReportPdfService>();
        services.AddScoped<IDateRangeAnimalsReportPdfService, DateRangeAnimalsReportPdfService>();
        services.AddScoped<ISelectedAnimalsReportPdfService, SelectedAnimalsReportPdfService>();
        services.AddScoped<IRepositoryDumpReportPdfService, RepositoryDumpReportPdfService>();

        services.Configure<BlobStorageSettings>(options =>
        {
            options.ConnectionString = configuration["BlobStorage:ConnectionString"];
            options.ContainerName = configuration["BlobStorage:ContainerName"]!;
            options.AccountName = configuration["BlobStorage:AccountName"]!;
        });
        services.AddSingleton<IBlobStorageService, BlobStorageService>();
    }

    public async Task MigrateAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AnimalsDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}