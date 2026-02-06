using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;

namespace AnimalRegistry;

public static class OpenApiConfiguration
{
    public static IServiceCollection AddAuth0OpenApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOpenApi(options =>
        {
            var auth0Domain = configuration["Auth0:Domain"]?.TrimEnd('/');
            var auth0Audience = configuration["Auth0:Audience"];

            options.AddDocumentTransformer((document, _, _) =>
            {
                document.SecurityRequirements = new List<OpenApiSecurityRequirement>
                {
                    new()
                    {
                        [
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme, Id = "oauth2",
                                },
                            }
                        ] = new List<string>(),
                    },
                };

                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes = new Dictionary<string, OpenApiSecurityScheme>
                {
                    ["oauth2"] = new()
                    {
                        Type = SecuritySchemeType.OAuth2,
                        Flows = new OpenApiOAuthFlows
                        {
                            AuthorizationCode = new OpenApiOAuthFlow
                            {
                                AuthorizationUrl =
                                    new Uri(
                                        $"{auth0Domain}/authorize?audience={Uri.EscapeDataString(auth0Audience ?? "")}"),
                                TokenUrl = new Uri($"{auth0Domain}/oauth/token"),
                                Scopes = new Dictionary<string, string>
                                {
                                    ["openid"] = "OpenID",
                                    ["profile"] = "Profile",
                                    ["email"] = "Email",
                                    ["offline_access"] = "Offline Access",
                                },
                            },
                        },
                    },
                };

                return Task.CompletedTask;
            });
        });

        return services;
    }

    public static IEndpointRouteBuilder MapScalarWithAuth0(this IEndpointRouteBuilder endpoints,
        IConfiguration configuration)
    {
        var auth0ClientId = configuration["Auth0:ClientId"];

        endpoints.MapScalarApiReference(options =>
        {
            options
                .AddPreferredSecuritySchemes("oauth2")
                .AddOAuth2Flows("oauth2", flows =>
                {
                    flows.AuthorizationCode = new AuthorizationCodeFlow
                    {
                        ClientId = auth0ClientId, Pkce = Pkce.Sha256,
                    };
                })
                .AddDefaultScopes("oauth2", "openid", "profile", "email", "offline_access");
        });

        return endpoints;
    }
}