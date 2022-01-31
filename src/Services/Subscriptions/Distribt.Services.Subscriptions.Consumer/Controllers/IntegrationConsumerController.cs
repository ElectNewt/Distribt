using Distribt.Shared.Communication.Consumer.Host;
using Distribt.Shared.Communication.Consumer.Manager;
using Microsoft.AspNetCore.Mvc;

namespace Distribt.Services.Subscriptions.Consumer.Controllers;

[ApiController]
[Route("[controller]")]
public class IntegrationConsumerController : ConsumerController<IntegrationMessage>
{
    public IntegrationConsumerController(IConsumerManager<IntegrationMessage> consumerManager) : base(consumerManager)
    {
    }
}