using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Threading.RateLimiting;

namespace AnimalRegistry.Modules.Contact.RateLimiting;

public static class ContactRateLimiting
{
    public const string PolicyName = "contact-form";

    public static IServiceCollection AddContactRateLimiter(
        this IServiceCollection services,
        ContactRateLimitSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        return services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy(PolicyName, httpContext => RateLimitPartition.GetFixedWindowLimiter(
                GetPartitionKey(httpContext),
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = settings.PermitLimit,
                    Window = TimeSpan.FromMinutes(settings.WindowMinutes),
                    QueueLimit = 0,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    AutoReplenishment = true,
                }));
        });
    }

    private static string GetPartitionKey(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var address = httpContext.Connection.RemoteIpAddress;
        if (address is null)
        {
            return "unknown";
        }

        if (address.IsIPv4MappedToIPv6)
        {
            address = address.MapToIPv4();
        }

        return address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6
            ? GetIPv6PrefixKey(address)
            : address.ToString();
    }

    private static string GetIPv6PrefixKey(IPAddress address)
    {
        var bytes = address.GetAddressBytes();
        Array.Clear(bytes, 8, 8);

        return new IPAddress(bytes).ToString() + "/64";
    }
}