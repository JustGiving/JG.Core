namespace JG.Core.Logging.Test;

public class LogEvent
{
    public required string type { get; set; }
    public required string timestamp { get; set; }
    public required string level { get; set; }
    public required int levelNumber { get; set; }
    public required string message { get; set; }
    public required LogEventApp app { get; set; }
    public required string environment { get; set; }
    public string? correlationId { get; set; }
    public required LogEventInfra infra { get; set; }
    public LogEventReq? req { get; set; }
    public Dictionary<string, object>? properties { get; set; }
}

public class LogEventApp
{
    public required string name { get; set; }
    public required string version { get; set; }
}

public class LogEventInfra
{
    public required string type { get; set; }
    public required string instance { get; set; }
}

public class LogEventReq
{
    public required string method { get; set; }
    public required string path { get; set; }
    public string? queryString { get; set; }
    public Dictionary<string, string>? headers { get; set; }
    public string? referer { get; set; }
    public string? clientIp { get; set; }
}
