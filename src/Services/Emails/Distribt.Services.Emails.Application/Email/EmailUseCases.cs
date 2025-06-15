namespace Distribt.Services.Emails.Application.Email;

public class EmailUseCases
{
    public ISendEmailUseCase SendEmail { get; }

    public EmailUseCases(ISendEmailUseCase sendEmailUseCase)
    {
        SendEmail = sendEmailUseCase;
    }
}
