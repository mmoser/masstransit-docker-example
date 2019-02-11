using System;

namespace ServiceBus.Messages
{
  public class LogGreetingMessage 
  {
    public Guid CorrelationId { get; set; }
    public string Greeting { get; set; }
  }
}
