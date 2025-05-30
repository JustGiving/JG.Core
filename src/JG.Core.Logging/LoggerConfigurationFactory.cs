using Destructurama;
using JG.Core.Logging.Enrichers;
using JG.Core.Logging.Formatters;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace JG.Core.Logging;

public static class LoggerConfigurationFactory
{
    public static LoggerConfiguration Create()
    {
        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Override(
                "Microsoft",
                minimumLevel: Env.IsDeployed() ? LogEventLevel.Warning : LogEventLevel.Information
            )
            .MinimumLevel.Override(
                "System",
                minimumLevel: Env.IsDeployed() ? LogEventLevel.Warning : LogEventLevel.Information
            )
            .MinimumLevel.ControlledBy(new LoggingLevelSwitch(Env.GetLogEventLevel()))
            .Enrich.FromLogContext()
            .Enrich.With<AppInfoEnricher>()
            .Enrich.With<EnvironmentEnricher>()
            .Enrich.With<InfrastructureInfoEnricher>()
            .Destructure.UsingAttributes();

        if (Env.GetLogFormat() == LogFormat.Json || Env.IsDeployed())
        {
            loggerConfig.WriteTo.Console(new JsonFormatter());
        }
        else
        {
            loggerConfig.WriteTo.Console();
        }

        return loggerConfig;
    }
}
