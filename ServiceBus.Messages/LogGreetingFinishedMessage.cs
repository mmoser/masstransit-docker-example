using System;

namespace ServiceBus.Messages
{
  public class LogGreetingFinishedMessage
  {
    public Guid CorrelationId { get; set; }
  }
}
