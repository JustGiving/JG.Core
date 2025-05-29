# JG.Core

A collection of core libraries to use for .NET Core services.

Provided libraries:
 - [`JG.Core.Logging`](./src/JG.Core.Logging/) - the logging integration library. Configures Serilog with the JSON formatter and the necessary enrichers. Provides the `ConfigureJustGivingLogging` extension method on the `Microsoft.Extensions.Hosting.IHostBuilder` interface.
 - [`JG.Core.Logging.Formatters`](./src/JG.Core.Logging.Formatters/) - provides the JSON Serilog formatter.
 - [`JG.Core.Info`](./src/JG.Core.Info/) - provides helper classes exposing information about the service such as service name, version, the current environment and hosting platform (EKS, Serverless)
 - [`JG.Core.Hosting`](./src/JG.Core.Hosting/) - (experimental) Provides the `ConfigureJustGivingHost` extension method on the `Microsoft.Extensions.Hosting.IHostBuilder` interface, which configures all the other supported integrations. Currently only supports logging.
