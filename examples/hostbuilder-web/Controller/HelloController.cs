using Microsoft.AspNetCore.Mvc;
using ILogger = Serilog.ILogger;

namespace JG.Core.Example.HostBuilder.Web.HelloController;

[Route("hello")]
public class HelloController : Controller
{
    private readonly ILogger _logger;

    public HelloController(ILogger logger)
    {
        _logger = logger;
    }

    [HttpGet("world")]
    public string World()
    {
        _logger.Information("hello world");

        return "hello world";
    }
}
