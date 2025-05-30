using ILogger = Serilog.ILogger;

namespace JG.Core.Example.HostBuilder.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger _logger;

    public Worker(ILogger logger)
    {
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.Information("Worker running");

        return Task.CompletedTask;
    }
}
