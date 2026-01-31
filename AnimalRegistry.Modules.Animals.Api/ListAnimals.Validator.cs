using FastEndpoints;
using FluentValidation;

namespace AnimalRegistry.Modules.Animals.Api;

internal sealed class ListAnimalsValidator : Validator<ListAnimalsRequest>;
