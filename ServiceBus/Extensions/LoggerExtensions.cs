using System;
using Microsoft.Extensions.Logging;

namespace ServiceBus.Extensions
{
  public static class LoggerExtensions
  {
    public static void LogException(this ILogger logger, Exception ex, string message, params object[] messageArgs)
    {
      logger.LogError(
        new EventId(),
        ex,
        message,
        messageArgs);
    }

    public static void LogException(this ILogger logger, string message, Exception ex, params object[] messageArgs)
    {
      logger.LogError(
        new EventId(),
        ex,
        message,
        messageArgs);
    }
  }
}
