using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ninject;
using ServiceBus.Messages;

namespace ServiceBus
{
  class Program
  {
    private static IBusControl _busControl;
    private static readonly ManualResetEventSlim Done = new ManualResetEventSlim(false);

    static int Main(string[] args)
    {
      var task = Task.Run(() => MainAsync(args));
      task.Wait();

      return task.Result;
    }

    static async System.Threading.Tasks.Task<int> MainAsync(string[] args)
    {
      StandardKernel kernel;
      try
      {
        kernel = new StandardKernel(new NinjectServiceBusModule());
      }
      catch (Exception e)
      {
        Console.WriteLine("Failed to get register the kernel");
        throw;
      }

      var logger = kernel.Get<ILogger<Program>>();
      try
      {
        using (var cts = new CancellationTokenSource())
        {
          _busControl = kernel.Get<IBusControl>();
          void CancelAction()
          {
            if (!cts.IsCancellationRequested)
            {
              logger.LogInformation("Application is shutting down...");
              _busControl.Stop();
              cts.Cancel();
            }
          }

          Console.CancelKeyPress += (sender, eventArgs) =>
          {
            CancelAction();
            eventArgs.Cancel = true;
          };

          logger.LogInformation("Application is starting...");

          await _busControl.StartAsync(cts.Token);
          Done.Set();

          var configurationRoot = kernel.Get<IConfigurationRoot>();
          var runTest = Convert.ToBoolean(configurationRoot["RunTest"]);
          if (runTest)
          {
            while (!cts.IsCancellationRequested)
            {
              var date = DateTime.Now;
              var message = new HelloWorldMessage()
              {
                CorrelationId = Guid.NewGuid(),
                Greeting = $"Hello, the time is {date.ToString("HH:mm:ss tt zz")}"
              };

              await _busControl.Publish(message, cts.Token);
              await Task.Delay(5000);
            }
          }

          return 0;
        }
      }
      catch (Exception exception)
      {
        logger.LogError(exception, "Failed to run the service bus");
        throw;
      }
    }
  }
}
