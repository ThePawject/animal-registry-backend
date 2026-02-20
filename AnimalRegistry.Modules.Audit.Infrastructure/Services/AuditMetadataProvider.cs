using AnimalRegistry.Modules.Audit.Application.Services;
using AnimalRegistry.Modules.Audit.Domain.AuditEntries;
using AnimalRegistry.Shared.Access;
using Microsoft.AspNetCore.Http;

namespace AnimalRegistry.Modules.Audit.Infrastructure.Services;

public sealed class AuditMetadataProvider(
    ICurrentUser currentUser,
    IHttpContextAccessor httpContextAccessor)
    : IAuditMetadataProvider
{
    public AuditMetadata GetMetadata()
    {
        var httpContext = httpContextAccessor.HttpContext;

        var userId = currentUser?.UserId ?? "System";
        var email = currentUser?.Email ?? "system@system";
        var shelterId = currentUser?.ShelterId ?? "N/A";

        var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString();
        var userAgent = httpContext?.Request.Headers.UserAgent.ToString();

        return AuditMetadata.Create(userId, email, shelterId, ipAddress, userAgent);
    }
}