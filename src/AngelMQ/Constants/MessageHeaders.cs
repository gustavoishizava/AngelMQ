namespace AngelMQ.Constants;

public static class MessageHeaders
{
    public const string MessageTTL = "x-message-ttl";
    public const string Retries = "x-retries";
    public const string ExceptionRootCause = "x-exception-root-cause";
    public const string ExceptionStackTrace = "x-exception-stacktrace";
    public const string DeadLetterExchange = "x-dead-letter-exchange";
}
