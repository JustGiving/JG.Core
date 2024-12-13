using Serilog.Core;
using Serilog.Events;
using System.Reflection;

namespace JG.Core.Logging.Enrichers;

internal class AppInfoEnricher : ILogEventEnricher
{
    public const string AppPropertyName = "app";

    private readonly string _name;
    private readonly string _version;
    private readonly string? _deployable;

    public AppInfoEnricher()
    {
        _name = Environment.GetEnvironmentVariable("SERVICE_NAME") ?? "unknown";
        _version = Environment.GetEnvironmentVariable("SERVICE_VERSION") ?? Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unknown";
        _deployable = Environment.GetEnvironmentVariable("DEPLOYABLE_NAME");
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var properties = new List<LogEventProperty> {
            new("name", new ScalarValue(_name)),
            new ("version", new ScalarValue(_version)),
        };

        if (_deployable != null)
        {
            properties.Add(new("deployable", new ScalarValue(_deployable)));
        }

        logEvent.AddPropertyIfAbsent(new LogEventProperty(AppPropertyName, new StructureValue(properties)));
    }
}
