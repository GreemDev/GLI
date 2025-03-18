using GitLabCli.Entities;
using GitLabCli.Helpers;

namespace GitLabCli.Entities.EventArgs;

public class LogEventArgs
{
    public LogSeverity Severity { get; init; }
    public LogSource Source { get; init; }
    public string? Message { get; init; }
    public Exception? Error { get; init; }
    public InvocationInfo Invocation { get; init; } = default;
}