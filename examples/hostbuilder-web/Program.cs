using JG.Core.Logging;
using Serilog.Events;

namespace JG.Core.Example.HostBuilder.Web;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        var loggerConfiguration = LoggerConfigurationFactory.Create();

        loggerConfiguration.MinimumLevel.Override(
            "Microsoft.AspNetCore.DataProtection",
            minimumLevel: LogEventLevel.Error
        );

        return Host.CreateDefaultBuilder(args)
            .ConfigureJustGivingLogging(loggerConfiguration.CreateLogger())
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
    }
}
