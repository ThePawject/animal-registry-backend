using AnimalRegistry.Modules.Contact.Infrastructure.Email;
using System.Collections.Concurrent;

namespace AnimalRegistry.Modules.Animals.Tests.Functional.Fixture;

public sealed class RecordingEmailSender : IEmailSender
{
    private readonly ConcurrentQueue<EmailMessage> _messages = new();

    public IReadOnlyCollection<EmailMessage> Messages => _messages.ToArray();

    public Exception? FailWith { get; set; }

    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        if (FailWith is not null)
        {
            return Task.FromException(FailWith);
        }

        _messages.Enqueue(message);

        return Task.CompletedTask;
    }

    public void Reset()
    {
        _messages.Clear();
        FailWith = null;
    }
}