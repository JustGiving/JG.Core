using Serilog.Core;
using Serilog.Events;

namespace JG.Core.Logging.Enrichers;

internal class InfrastructureInfoEnricher : ILogEventEnricher
{
    public const string InfraPropertyName = "infra";

    private readonly string _type;
    private readonly string _instance;

    public InfrastructureInfoEnricher()
    {
        if (Environment.GetEnvironmentVariable("EKS") == "true")
        {
            _type = "eks";
            _instance = Environment.GetEnvironmentVariable("HOSTNAME") ?? "unknown";
        }
        else if (Environment.GetEnvironmentVariable("AWS_LAMBDA_FUNCTION_NAME") != null)
        {
            _type = "serverless";
            _instance = Environment.GetEnvironmentVariable("AWS_LAMBDA_FUNCTION_NAME") ?? "unknown";
        }
        else
        {
            _type = "unknown";
            _instance = "unknown";
        }
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        logEvent.AddPropertyIfAbsent(new LogEventProperty(InfraPropertyName, new StructureValue(
            [
                new LogEventProperty("type", new ScalarValue(_type)),
                new LogEventProperty("instance", new ScalarValue(_instance)),
            ]
        )));
    }
}
