using Distribt.Services.Emails.Application.DataAccess;
using Distribt.Services.Emails.Controllers;

namespace Distribt.Services.Emails.Application.Email;

public interface ISendEmailUseCase
{
    Task<bool> Execute(EmailDto emailDto);
}

public class SendEmailUseCase : ISendEmailUseCase
{
    private readonly IEmailRepository _repository;
    private readonly IEmailProvider _provider;

    public SendEmailUseCase(IEmailRepository repository, IEmailProvider provider)
    {
        _repository = repository;
        _provider = provider;
    }

    public async Task<bool> Execute(EmailDto emailDto)
    {
        string id = await _repository.AddEmail(emailDto);
        await _provider.Send(emailDto);
        await _repository.MarkAsSent(id);
        return true;
    }
}
