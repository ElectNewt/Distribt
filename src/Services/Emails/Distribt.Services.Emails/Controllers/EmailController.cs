using Microsoft.AspNetCore.Mvc;
using Distribt.Services.Emails.Application.Email;

namespace Distribt.Services.Emails.Controllers;
[ApiController]
[Route("[controller]")]
public class EmailController
{
    private readonly EmailUseCases _emailUseCases;

    public EmailController(EmailUseCases emailUseCases)
    {
        _emailUseCases = emailUseCases;
    }

    [HttpPost(Name = "send")]
    public Task<bool> Send(EmailDto emailDto)
    {
        return _emailUseCases.SendEmail.Execute(emailDto);
    }
}

public record EmailDto(string from, string to, string subject, string body);


