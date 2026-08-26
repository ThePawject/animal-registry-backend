using AnimalRegistry.Modules.Contact.Application;
using AnimalRegistry.Modules.Contact.RateLimiting;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AnimalRegistry.Modules.Contact.Api;

internal sealed class SubmitContactRequest(IMediator mediator, ILogger<SubmitContactRequest> logger)
    : Endpoint<SubmitContactRequestBody>
{
    public override void Configure()
    {
        Post(SubmitContactRequestBody.Route);
        AllowAnonymous();

        DontThrowIfValidationFails();

        Options(x => x.RequireRateLimiting(ContactRateLimiting.PolicyName));
        Description(x => x
            .Produces(StatusCodes.Status202Accepted)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status429TooManyRequests));
        Summary(x =>
        {
            x.Summary = "Submits the public contact form.";
            x.Description = "Stores the submission and notifies the team mailbox. Rate limited per client IP.";
        });
    }

    public override async Task HandleAsync(SubmitContactRequestBody req, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(req);

        if (!string.IsNullOrWhiteSpace(req.Honey))
        {
            logger.LogInformation("Contact form honeypot was filled in - the submission was discarded.");
            await HttpContext.Response.SendStatusCodeAsync(StatusCodes.Status202Accepted, ct);

            return;
        }

        if (ValidationFailed)
        {
            await HttpContext.Response.SendErrorsAsync(ValidationFailures, cancellation: ct);

            return;
        }

        var result = await mediator.Send(
            new SubmitContactRequestCommand(
                req.ShelterName.Trim(),
                req.ContactPerson.Trim(),
                req.Email.Trim(),
                NullIfBlank(req.Phone),
                NullIfBlank(req.Message)),
            ct);

        if (result.IsFailure)
        {
            await this.SendResultAsync(result, ct);

            return;
        }

        await HttpContext.Response.SendStatusCodeAsync(StatusCodes.Status202Accepted, ct);
    }

    private static string? NullIfBlank(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}