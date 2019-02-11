using System;
using Automatonymous;
using ServiceBus.Messages;
using ServiceBus.SagaInstances;

namespace ServiceBus.Subscribers.HelloWorld
{
  public class HelloWorldSaga : MassTransitStateMachine<HelloWorldInstance>
  {
    public Event<HelloWorldMessage> HelloWorldReceived { get; set; }
    public Event<LogGreetingFinishedMessage> LogGreetingFinished { get; set; }

    public State LoggingGreeting { get; set; }

    public HelloWorldSaga()
    {
      InstanceState(instance => instance.CurrentState); // Tells the Saga where to store the state that it is in.

      // Tells the Saga what field to compare from an existing instance to the field on the message. If it cannot find an existing instance, it will create a new instance if this is a message that starts the saga.
      Event(() => HelloWorldReceived,
        x => x.CorrelateById(instance => instance.CorrelationId, context => context.Message.CorrelationId));

      Event(() => LogGreetingFinished,
        x => x.CorrelateById(instance => instance.CorrelationId, context => context.Message.CorrelationId));

      // State information. Initially is a built in state.
      Initially(
        When(HelloWorldReceived)
          .Then(context =>
          {
            var date = DateTime.Now;
            context.Instance.Created = date;
            context.Instance.LastUpdated = date;
            context.Instance.Greeting = context.Data.Greeting;
          })
          .TransitionTo(LoggingGreeting)
          .Then(context =>
          {
            context.Publish(new LogGreetingMessage()
            {
              CorrelationId = context.Instance.CorrelationId,
              Greeting = context.Instance.Greeting
            });
          })
        );

      During(LoggingGreeting,
        When(LogGreetingFinished)
          .Then(context =>
          {
            context.Instance.LastUpdated = DateTime.Now;
          })
          .Finalize()
        );

      //During(Final,
        
      //  );
    }
  }
}
