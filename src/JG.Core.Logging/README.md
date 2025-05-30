# JG.Core.Logging

The Serilog logging integration library for JustGiving services. Configures Serilog with the JSON formatter and the necessary enrichers.

## Installation

```bash
dotnet add package JG.Core.Logging
```

## Usage

The library provides the `ConfigureJustGivingLogging` extension method on the `Microsoft.Extensions.Hosting.IHostBuilder` interface:

```cs
using JG.Core.Logging;

public class Program
{
    ...

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureJustGivingLogging()
            ...
}
```

This call will initialise a Serilog logger instance, set it on `Serilog.Log.Logger` and configure it for dependency injection. The logger will be configured to output the logs in the JSON format when deployed or in the standard console format when running locally. A number of enrichers are automatically included to add context-specific information, such as service name, version, the current environment etc.

Additionally, it's possible to override some of the configuration by creating a logger instance manually and then passing it to `ConfigureJustGivingLogging`:

```cs
using JG.Core.Logging;

public class Program
{
    ...

    public static IHostBuilder CreateHostBuilder(string[] args) {
      var loggerConfiguration = LoggerConfigurationFactory.Create();

      // change settings on loggerConfiguration, e.g. add additional enrichers

      return Host.CreateDefaultBuilder(args)
        .ConfigureJustGivingLogging(loggerConfiguration.CreateLogger())
        ...
    }
}
```

### ASP.NET Core Middleware

The library provides an ASP.NET Core middleware that enriches log messages with request details.

To enable the middleware:

```cs
app.UseMiddleware<RequestLogger>();
```

Additional options can be provided to the middleware by using the `AddRequestLoggerOptions` extension method on `IServiceCollection`:

```cs
services.AddRequestLoggerOptions(options =>
{
    options.LogHandlingRequest = true;
});
```
