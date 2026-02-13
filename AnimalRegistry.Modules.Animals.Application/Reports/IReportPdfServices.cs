using AnimalRegistry.Modules.Animals.Application.Reports.Models;

namespace AnimalRegistry.Modules.Animals.Application.Reports;

public interface IRepositoryDumpReportPdfService
{
    byte[] GenerateReport(RepositoryDumpReportData data, DateTimeOffset generatedAt);
}

public interface ISelectedAnimalsReportPdfService
{
    byte[] GenerateReport(SelectedAnimalsReportData data, DateTimeOffset generatedAt);
}

public interface IDateRangeAnimalsReportPdfService
{
    byte[] GenerateReport(DateRangeAnimalsReportData data, DateTimeOffset generatedAt);
}

public interface IEventReportPdfService
{
    byte[] GenerateReport(EventReportData data, DateTimeOffset generatedAt);
}