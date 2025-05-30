namespace JG.Core.Info;

public static class EnvInfo
{
    public static readonly string Env =
        Environment.GetEnvironmentVariable("DEPLOY_ENV")
        ?? Environment.GetEnvironmentVariable("ENVIRONMENT")
        ?? "local";
}
