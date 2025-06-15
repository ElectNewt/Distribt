using Distribt.Services.Emails.Controllers;

namespace Distribt.Services.Emails.Application.Services;

public interface IEmailProvider
{
    Task Send(EmailDto email, CancellationToken cancellationToken = default);
}

public class FakeEmailProvider : IEmailProvider
{
    public Task Send(EmailDto email, CancellationToken cancellationToken = default)
    {
        // Fake provider does nothing
        return Task.CompletedTask;
    }
}
