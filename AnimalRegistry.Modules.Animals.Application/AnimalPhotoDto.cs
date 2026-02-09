namespace AnimalRegistry.Modules.Animals.Application;

public sealed record AnimalPhotoDto(
    Guid Id,
    string Url,
    string FileName,
    DateTimeOffset UploadedOn
);
