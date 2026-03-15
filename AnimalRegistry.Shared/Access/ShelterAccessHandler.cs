using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AnimalRegistry.Shared.Access;

public sealed class ShelterAccessHandler
    : AuthorizationHandler<ShelterAccessRequirement>
{
    public const string ShelterRolePrefix = "Shelter_Access_";
    public const string ShelterIdClaimType = "shelter_id";
    public const string ShelterAccessPolicyName = "ShelterAccess";

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ShelterAccessRequirement requirement)
    {
        var roleClaimType =
            (context.User.Identity as ClaimsIdentity)?.RoleClaimType
            ?? ClaimTypes.Role;

        var shelterRoles = context.User.FindAll(roleClaimType)
            .Select(c => c.Value)
            .Where(v => v.StartsWith(ShelterRolePrefix, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (shelterRoles.Count == 0)
        {
            context.Fail(new AuthorizationFailureReason(
                this,
                "Missing shelter access role (expected exactly one role starting with 'Shelter_Access_')."
            ));
            return Task.CompletedTask;
        }

        if (shelterRoles.Count > 1)
        {
            context.Fail(new AuthorizationFailureReason(
                this,
                "Multiple shelter access roles found; exactly one is required."
            ));
            return Task.CompletedTask;
        }

        var shelterRole = shelterRoles.First();
        var shelterId = shelterRole[ShelterRolePrefix.Length..];

        var identity = (ClaimsIdentity)context.User.Identity!;
        identity.AddClaim(new Claim(ShelterIdClaimType, shelterId));

        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}