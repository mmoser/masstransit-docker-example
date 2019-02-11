using System;
using System.Threading.Tasks;
using MassTransit;
using ServiceBus.Messages;

namespace ServiceBus.Subscribers.HelloWorld
{
    public class ConsoleLogGreetingConsumer : IConsumer<HelloWorldMessage>
    {
      public Task Consume(ConsumeContext<HelloWorldMessage> context)
      {
        return Task.Factory.StartNew(() => {
          var message = context.Message;

          Console.WriteLine(message.Greeting);
        });
      }
    }
}
