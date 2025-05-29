using JG.Core.Info;
using Serilog.Core;
using Serilog.Events;

namespace JG.Core.Logging.Enrichers;

internal class InfrastructureInfoEnricher : ILogEventEnricher
{
    public const string InfraPropertyName = "infra";

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        logEvent.AddPropertyIfAbsent(
            new LogEventProperty(
                InfraPropertyName,
                new StructureValue(
                    [
                        new LogEventProperty("type", new ScalarValue(InfrastructureInfo.Type)),
                        new LogEventProperty(
                            "instance",
                            new ScalarValue(InfrastructureInfo.Instance)
                        ),
                    ]
                )
            )
        );
    }
}
