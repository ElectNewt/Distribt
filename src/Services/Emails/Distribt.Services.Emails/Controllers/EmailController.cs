using Microsoft.AspNetCore.Mvc;

namespace Distribt.Services.Emails.Controllers;
[ApiController]
[Route("[controller]")]
public class EmailController
{
    [HttpPost(Name = "send")]
    public Task<bool> Send(EmailDto emailDto)
    {
        //TODO: logic to send the email.
        return Task.FromResult(true);
    }
}

public record EmailDto(string from, string to, string subject, string body);

