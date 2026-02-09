using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AnimalRegistry.Modules.Animals.Tests.Functional;

public sealed class TestJwtTokenGenerator(
    string issuer = "https://test-auth.com/",
    string audience = "https://test-api.com/")
{
    private readonly SymmetricSecurityKey _signingKey = new("test-secret-key-at-least-32-characters-long"u8.ToArray());
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    public string GenerateToken(TestUser user)
    {
        var claims = new List<Claim>(user.Roles.Count + user.CustomClaims.Count + 4)
        {
            new("https://ThePawject/user_id", TestUser.UserId),
            new(JwtRegisteredClaimNames.Name, TestUser.Name),
            new("https://ThePawject/email", TestUser.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        claims.AddRange(user.Roles.Select(role => new Claim("https://ThePawject/roles", role)));
        claims.AddRange(user.CustomClaims.Select(claim => new Claim(claim.Key, claim.Value)));

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256)
        );

        return _tokenHandler.WriteToken(token);
    }

    public TokenValidationParameters GetTokenValidationParameters()
    {
        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = _signingKey,
            RoleClaimType = "https://ThePawject/roles",
        };
    }
}