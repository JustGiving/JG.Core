using JG.Core.Logging;

namespace JG.Core.Example.HostBuilder.Worker;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureJustGivingLogging()
            .ConfigureServices(services =>
            {
                services.AddHostedService<Worker>();
            });
}
