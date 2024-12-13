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
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", minimumLevel: LogEventLevel.Warning)
            .MinimumLevel.ControlledBy(new LoggingLevelSwitch(Env.GetLogEventLevel()))
            .Enrich.FromLogContext()
            .Enrich.With<AppInfoEnricher>()
            .Enrich.With<EnvironmentEnricher>()
            .Enrich.With<InfrastructureInfoEnricher>()
            .Destructure.UsingAttributes();

        if (Env.IsDeployed() || Env.GetLogFormat() == LogFormat.Json)
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
