using Microsoft.Extensions.DependencyInjection;

namespace JG.Core.Logging.Middleware
{
    public static class RequestLoggerOptionsExtensions
    {
        public static IServiceCollection AddRequestLoggerOptions(this IServiceCollection service, Action<RequestLoggerOptions>? options = null)
        {
            options ??= (opts => { });

            service.Configure(options);
            return service;
        }
    }
}
