using System;
using System.Collections.Generic;
using Automatonymous;
using GreenPipes;
using MassTransit;
using MassTransit.MongoDbIntegration.Saga;
using MassTransit.MongoDbIntegration.Saga.Context;
using MassTransit.NinjectIntegration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;
using Ninject;
using Ninject.Modules;
using Serilog;
using ServiceBus.Extensions;
using ServiceBus.SagaInstances;
using ServiceBus.Subscribers.HelloWorld;
using ServiceBus.Subscribers.LogGreeting;

namespace ServiceBus
{
  public class NinjectServiceBusModule : NinjectModule
  {
    private IConfigurationRoot _configurationRoot;
    private ILogger<NinjectServiceBusModule> _logger;
    private ILoggerFactory _loggerFactory;
    private readonly List<Action<IReceiveEndpointConfigurator>> _sagaActions = new List<Action<IReceiveEndpointConfigurator>>();
    private readonly List<Action<IReceiveEndpointConfigurator>> _consumerActions = new List<Action<IReceiveEndpointConfigurator>>();
    private bool _enableSagas = false;

    public override void Load()
    {
      var builder = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

      builder.AddEnvironmentVariables();
      _configurationRoot = builder.Build();

      Kernel.Bind<IConfigurationRoot>().ToMethod(context => _configurationRoot);
      _enableSagas = Convert.ToBoolean(_configurationRoot["enableSagas"]);

      CreateLoggerAndRegister();
      RegisterServices();

      if (_enableSagas)
      {
        RegisterRepositories();
      }

      RegisterServiceBus();
    }

    private void CreateLoggerAndRegister()
    {
      Kernel.Bind<ILoggerFactory>().To<LoggerFactory>().InSingletonScope();
      Kernel.Bind(typeof(ILogger<>)).To(typeof(Logger<>)).InSingletonScope();

      _loggerFactory = Kernel.Get<ILoggerFactory>();
      _logger = Kernel.Get<ILogger<NinjectServiceBusModule>>();

      _loggerFactory.AddSerilog(new LoggerConfiguration()
        .ReadFrom.Configuration(_configurationRoot)
        .CreateLogger());
    }

    private void RegisterServices()
    {
      // Register the services that we need here.
    }

    private void RegisterServiceBus()
    {
      _logger.LogInformation("Registering the Service Bus");

      RegisterConsumers();

      if (_enableSagas)
      {
        RegisterSagas();
      }

      Kernel.Bind<IBusControl, IBus>().ToMethod(context =>
      {
        var rabbitMqConnection = _configurationRoot.GetConnectionString("RabbitMQConnection");
        var bus = Bus.Factory.CreateUsingRabbitMq(configurator =>
        {
          var host = configurator.Host(new Uri(rabbitMqConnection), hostConfigurator =>
          {
            hostConfigurator.Username("guest");
            hostConfigurator.Password("guest");
          });

          configurator.UseRetry(retryConfigurator => retryConfigurator.Exponential(20, TimeSpan.FromMilliseconds(200), TimeSpan.FromMinutes(10), TimeSpan.FromMilliseconds(200)));

          configurator.ReceiveEndpoint(host, "servicebus_queue", endpointConfigurator =>
          {
            LoadConsumers(endpointConfigurator);
            LoadSagas(endpointConfigurator);
          });
        });

        return bus;
      }).InSingletonScope();
    }

    private void RegisterRepositories()
    {
      var mongoUrl = _configurationRoot.GetConnectionString("MongoConnection");
      var sagaDatabase = _configurationRoot["SagaDatabase"];

      Kernel.Bind<IMongoDatabase>()
        .ToMethod((ctx) =>
        {
          var mongoConnectionUrl = new MongoUrl(mongoUrl);
          var mongoClientSettings = MongoClientSettings.FromUrl(mongoConnectionUrl);
          var logger = ctx.Kernel.Get<ILogger<MongoClientSettings>>();

          mongoClientSettings.ConnectionMode = ConnectionMode.Direct;

          logger.LogInformation("Setting the timeouts");

          // From here: https://stackoverflow.com/questions/45742140/mongoerror-connection-timed-out-on-azure-cosmosdb
          // and "MongoDB Errors" found here: https://portal.azure.com/#@paylocity1.onmicrosoft.com/resource/subscriptions/c5fe6d3b-6f13-48a5-ad3e-e718ea449946/resourceGroups/NoSQL/providers/Microsoft.DocumentDb/databaseAccounts/pctyqa/troubleshoot  
          mongoClientSettings.SocketTimeout = TimeSpan.FromSeconds(10);
          mongoClientSettings.MaxConnectionIdleTime = TimeSpan.FromMinutes(2);
          mongoClientSettings.HeartbeatTimeout = TimeSpan.FromSeconds(5);

          logger.LogInformation("Subscribing to the events");
          mongoClientSettings.ClusterConfigurator = builder =>
          {
            builder.Subscribe<CommandFailedEvent>(e =>
            {
              logger.LogException(e.Failure, $"{e.CommandName} failed. The duration was {e.Duration}.");
            });
            builder.Subscribe<ConnectionFailedEvent>(e =>
            {
              logger.LogException(e.Exception, $"{e.ConnectionId} failed to connect with server id {e.ServerId}.");
            });
            builder.Subscribe<ConnectionOpeningFailedEvent>(e =>
            {
              logger.LogException(e.Exception, $"{e.ConnectionId} failed to open connection with server id {e.ServerId}.");
            });
            builder.Subscribe<ConnectionPoolCheckingOutConnectionFailedEvent>(e =>
            {
              logger.LogException(e.Exception, $"Failed to check out connection with server id {e.ServerId}.");
            });
            builder.Subscribe<ConnectionReceivingMessageFailedEvent>(e =>
            {
              logger.LogException(e.Exception, $"Connection failed to receive message server id {e.ServerId} and connection id {e.ConnectionId}.");
            });
            builder.Subscribe<ConnectionSendingMessagesFailedEvent>(e =>
            {
              logger.LogException(e.Exception, $"Connection failed to send message server id {e.ServerId} and connection id {e.ConnectionId}.");
            });
            builder.Subscribe<ServerHeartbeatFailedEvent>(e =>
            {
              logger.LogException(e.Exception, $"Server heartbeat failed to with server id {e.ServerId} and connection id {e.ConnectionId}.");
            });
          };

          var mongoClient = new MongoClient(mongoClientSettings);

          return mongoClient.GetDatabase(sagaDatabase);
        })
        .InSingletonScope()
        .Named("SagaDatabase");
    }

    private void RegisterConsumers()
    {
      RegisterConsumer<ConsoleLogGreetingConsumer>();
      RegisterConsumer<LogGreetingConsumer>();
    }

    private void LoadConsumers(IReceiveEndpointConfigurator configurator)
    {
      _consumerActions.ForEach(x => x.Invoke(configurator));
    }

    private void RegisterConsumer<TConsumer>()
      where TConsumer : class, IConsumer
    {
      Kernel.Bind<TConsumer>().ToSelf();

      _consumerActions.Add(configurator => configurator.Consumer(new NinjectConsumerFactory<TConsumer>(Kernel)));
    }

    private void RegisterSagas()
    {
      RegisterSaga<HelloWorldInstance, HelloWorldSaga>();
    }

    private void LoadSagas(IReceiveEndpointConfigurator configurator)
    {
      _sagaActions.ForEach(x => x.Invoke(configurator));
    }

    private void RegisterSaga<TInstance, TSaga>() 
      where TInstance : class, SagaStateMachineInstance, IVersionedSaga
      where TSaga : class, SagaStateMachine<TInstance> 
    {
      Kernel.Bind<TSaga>().ToSelf();
      var sagaDatabase = Kernel.Get<IMongoDatabase>("SagaDatabase");

      _sagaActions.Add(configurator => configurator.StateMachineSaga(Kernel.Get<TSaga>(), new MongoDbSagaRepository<TInstance>(sagaDatabase, new MongoDbSagaConsumeContextFactory(),  typeof(TInstance).Name)));
    }



  }
}
