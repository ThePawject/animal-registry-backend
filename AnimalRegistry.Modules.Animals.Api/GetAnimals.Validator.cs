using FluentValidation;
using FastEndpoints;

namespace AnimalRegistry.Modules.Animals.Api;

public sealed class GetAnimalsValidator : Validator<GetAnimalsRequest>;
