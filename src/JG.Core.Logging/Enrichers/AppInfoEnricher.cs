using Serilog.Core;
using Serilog.Events;
using System.Reflection;

namespace JG.Core.Logging.Enrichers;

internal class AppInfoEnricher : ILogEventEnricher
{
    public const string AppPropertyName = "app";

    private readonly string _name;
    private readonly string _version;
    private readonly string _deployable;

    public AppInfoEnricher()
    {
        var entryAssembly = Assembly.GetEntryAssembly();

        _name = Environment.GetEnvironmentVariable("SERVICE_NAME") ?? "unknown";
        _version = entryAssembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unknown";
        _deployable = Environment.GetEnvironmentVariable("DEPLOYABLE_NAME") ?? "unknown";
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        logEvent.AddPropertyIfAbsent(new LogEventProperty(AppPropertyName, new StructureValue(
            [
                new LogEventProperty("name", new ScalarValue(_name)),
                new LogEventProperty("version", new ScalarValue(_version)),
                new LogEventProperty("deployable", new ScalarValue(_deployable)),
            ]
        )));
    }
}
