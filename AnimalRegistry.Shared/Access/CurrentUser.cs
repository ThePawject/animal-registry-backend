using AnimalRegistry.Shared.Access;
using Microsoft.AspNetCore.Http;

namespace AnimalRegistry.Shared.CurrentUser;

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public string UserId => httpContextAccessor.HttpContext?.User.FindFirst("https://ThePawject/user_id")?.Value!;
    public string Email => httpContextAccessor.HttpContext?.User.FindFirst("https://ThePawject/email")?.Value!;

    public string ShelterId =>
        httpContextAccessor.HttpContext?.User.FindFirst(ShelterAccessHandler.ShelterIdClaimType)?.Value!;
}