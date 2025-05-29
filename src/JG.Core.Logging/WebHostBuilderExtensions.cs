using Microsoft.AspNetCore.Hosting;
using Serilog;

namespace JG.Core.Logging;

public static class WebHostBuilderExtensions
{
    public static IWebHostBuilder ConfigureLogging(
        this IWebHostBuilder builder,
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
