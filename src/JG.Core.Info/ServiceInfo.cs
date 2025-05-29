using System.Reflection;

namespace JG.Core.Info;

public static class ServiceInfo
{
    public static readonly string Name =
        Environment.GetEnvironmentVariable("SERVICE_NAME") ?? "unknown";
    public static readonly string Version =
        Environment.GetEnvironmentVariable("SERVICE_VERSION")
        ?? Assembly
            .GetEntryAssembly()
            ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion
        ?? "unknown";
    public static readonly string? Deployable = Environment.GetEnvironmentVariable(
        "DEPLOYABLE_NAME"
    );
}
