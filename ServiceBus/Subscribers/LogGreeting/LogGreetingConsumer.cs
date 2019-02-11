using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using ServiceBus.Messages;

namespace ServiceBus.Subscribers.LogGreeting
{
  public class LogGreetingConsumer : IConsumer<LogGreetingMessage>
  {
    private readonly ILogger<LogGreetingConsumer> _logger;

    public LogGreetingConsumer(ILogger<LogGreetingConsumer> logger)
    {
      _logger = logger;
    }

    public Task Consume(ConsumeContext<LogGreetingMessage> context)
    {
      var message = context.Message;

      _logger.LogInformation($"{message.CorrelationId}: {message.Greeting}");

      context.Publish(new LogGreetingFinishedMessage() {CorrelationId = message.CorrelationId});
      return Task.CompletedTask;
    }
  }
}
