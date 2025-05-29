using Microsoft.Extensions.Hosting;
using Serilog;

namespace JG.Core.Logging;

public static class HostBuilderExtensions
{
    public static IHostBuilder ConfigureJustGivingLogging(
        this IHostBuilder builder,
        ILogger? logger = null
    )
    {
        logger ??= LoggerConfigurationFactory.Create().CreateLogger();
        Log.Logger = logger;

        builder.ConfigureServices(services =>
        {
            services.AddSerilog(Log.Logger);
        });

        return builder;
    }
}
