using JG.Core.Logging;
using Microsoft.Extensions.Hosting;

namespace JG.Core.Hosting;

public static class HostBuilderExtensions
{
    public static IHostBuilder ConfigureJustGivingHost(this IHostBuilder builder)
    {
        builder.ConfigureJustGivingLogging();

        return builder;
    }
}
