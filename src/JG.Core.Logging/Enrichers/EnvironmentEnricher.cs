using Serilog.Core;
using Serilog.Events;

namespace JG.Core.Logging.Enrichers;

internal class EnvironmentEnricher : ILogEventEnricher
{
    public const string EnvironmentPropertyName = "environment";

    private readonly string _environment;

    public EnvironmentEnricher()
    {
        _environment = Environment.GetEnvironmentVariable("DEPLOY_ENV") ?? Environment.GetEnvironmentVariable("ENVIRONMENT") ?? "local";
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        logEvent.AddPropertyIfAbsent(new LogEventProperty(EnvironmentPropertyName, new ScalarValue(_environment)));
    }
}
