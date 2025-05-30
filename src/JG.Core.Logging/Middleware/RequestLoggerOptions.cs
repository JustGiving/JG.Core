namespace JG.Core.Logging.Middleware
{
    public class RequestLoggerOptions
    {
        [Obsolete(
            "This option is to provide backward compatibility with GG.Library.LoggingInitiator.Serilog, prefer using access logs instead"
        )]
        public bool LogHandlingRequest { get; set; }
    }
}
