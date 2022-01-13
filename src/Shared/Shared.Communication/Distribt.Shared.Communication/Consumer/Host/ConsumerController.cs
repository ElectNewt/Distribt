using Distribt.Shared.Communication.Consumer.Manager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Distribt.Shared.Communication.Consumer.Host;

public class ConsumerController<TMessage> : Controller
{
    private readonly IConsumerManager<TMessage> _consumerManager;

    public ConsumerController(IConsumerManager<TMessage> consumerManager)
    {
        _consumerManager = consumerManager;
    }
    
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Route("start")]
    public virtual IActionResult Start()
    {
        _consumerManager.RestartExecution();
        return Ok();
    }
}