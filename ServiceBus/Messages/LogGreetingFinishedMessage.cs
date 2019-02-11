using System;

namespace Paylocity.Communication.Feed.ServiceBus.Messages
{
  public class LogGreetingFinishedMessage
  {
    public Guid CorrelationId { get; set; }
  }
}
