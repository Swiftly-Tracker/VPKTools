namespace VPKTools.Tier0.Shared.Logging;

public interface ILoggingResponsePolicy
{
    LoggingResponse OnLog(LoggingContext context);
}
