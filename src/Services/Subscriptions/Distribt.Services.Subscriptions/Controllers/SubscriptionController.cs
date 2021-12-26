using Microsoft.AspNetCore.Mvc;

namespace Distribt.Services.Subscriptions.Controllers;

[ApiController]
[Route("[controller]")]
public class SubscriptionController
{
    [HttpPost(Name = "subscribe")]
    public Task<bool> Subscribe(SubscriptionDto subscription)
    {
        //TODO: logic 
        return Task.FromResult(true);
    }

    [HttpDelete(Name = "unsubscribe")]
    public Task<bool> Unsubscribe(SubscriptionDto subscription)
    {
        //TODO: logic 
        return Task.FromResult(true);
    }
}

public record SubscriptionDto(string email);
