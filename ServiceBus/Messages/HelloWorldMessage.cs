using System;

namespace Paylocity.Communication.Feed.ServiceBus.Messages
{
  public class HelloWorldMessage
  {
    public Guid CorrelationId { get; set; }
    public string Greeting { get; set; }
  }
}
