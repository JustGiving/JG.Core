using JG.Core.Info;
using Serilog.Core;
using Serilog.Events;

namespace JG.Core.Logging.Enrichers;

internal class EnvironmentEnricher : ILogEventEnricher
{
    public const string EnvironmentPropertyName = "environment";

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        logEvent.AddPropertyIfAbsent(
            new LogEventProperty(EnvironmentPropertyName, new ScalarValue(EnvInfo.Env))
        );
    }
}
