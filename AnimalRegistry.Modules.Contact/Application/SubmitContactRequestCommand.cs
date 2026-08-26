using AnimalRegistry.Shared;
using AnimalRegistry.Shared.MediatorPattern;

namespace AnimalRegistry.Modules.Contact.Application;

internal sealed record SubmitContactRequestCommand(
    string ShelterName,
    string ContactPerson,
    string Email,
    string? Phone,
    string? Message) : IRequest<Result>;