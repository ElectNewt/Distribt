using Distribt.Shared.Communication.Consumer.Host;
using Distribt.Shared.Communication.Consumer.Manager;
using Microsoft.AspNetCore.Mvc;

namespace Distribt.Services.Products.Consumer.Controllers;

[ApiController]
[Route("[controller]")]
public class DomainConsumerController : ConsumerController<DomainMessage>
{
    public DomainConsumerController(IConsumerManager<DomainMessage> consumerManager) : base(consumerManager)
    {
    }
}