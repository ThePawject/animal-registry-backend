using AnimalRegistry.Modules.Contact.Domain;
using AnimalRegistry.Modules.Contact.Infrastructure.Email;
using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;
using Microsoft.Extensions.Logging;

namespace AnimalRegistry.Modules.Contact.Application;

internal sealed class SubmitContactRequestCommandHandler(
    IContactRequestRepository repository,
    IContactEmailFactory emailFactory,
    IEmailSender emailSender,
    TimeProvider timeProvider,
    ILogger<SubmitContactRequestCommandHandler> logger)
    : IRequestHandler<SubmitContactRequestCommand, Result>
{
    public async Task<Result> Handle(SubmitContactRequestCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var contactRequest = ContactRequest.Create(
            request.ShelterName,
            request.ContactPerson,
            request.Email,
            request.Phone,
            request.Message,
            timeProvider.GetUtcNow());

        await repository.AddAsync(contactRequest, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        await NotifyTeamAsync(contactRequest, cancellationToken);

        return Result.Success();
    }

    private async Task NotifyTeamAsync(ContactRequest contactRequest, CancellationToken cancellationToken)
    {
        try
        {
            var message = emailFactory.Create(contactRequest);
            await emailSender.SendAsync(message, cancellationToken);
            contactRequest.MarkDelivered(timeProvider.GetUtcNow());

            logger.LogInformation(
                "Contact request {ContactRequestId} was accepted and the notification was sent.",
                contactRequest.Id);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            contactRequest.MarkDeliveryFailed(timeProvider.GetUtcNow(), exception.Message);

            logger.LogError(
                exception,
                "Contact request {ContactRequestId} was stored but the notification e-mail could not be sent.",
                contactRequest.Id);
        }

        await repository.SaveChangesAsync(cancellationToken);
    }
}