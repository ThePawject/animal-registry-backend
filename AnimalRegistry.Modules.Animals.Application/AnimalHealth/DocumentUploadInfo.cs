namespace AnimalRegistry.Modules.Animals.Application.AnimalHealth;

public sealed record DocumentUploadInfo(string FileName, Stream Content, string ContentType);
