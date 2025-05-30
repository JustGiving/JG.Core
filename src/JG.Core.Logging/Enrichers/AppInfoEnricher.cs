using JG.Core.Info;
using Serilog.Core;
using Serilog.Events;

namespace JG.Core.Logging.Enrichers;

internal class AppInfoEnricher : ILogEventEnricher
{
    public const string AppPropertyName = "app";

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var properties = new List<LogEventProperty>
        {
            new("name", new ScalarValue(ServiceInfo.Name)),
            new("version", new ScalarValue(ServiceInfo.Version)),
        };

        if (ServiceInfo.Deployable != null)
        {
            properties.Add(new("deployable", new ScalarValue(ServiceInfo.Deployable)));
        }

        logEvent.AddPropertyIfAbsent(
            new LogEventProperty(AppPropertyName, new StructureValue(properties))
        );
    }
}
