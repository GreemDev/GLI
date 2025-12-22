using System.Diagnostics.CodeAnalysis;
using gli.Helpers;
using Gommon;

namespace gli.CommandLib;

[SuppressMessage("Trimming",
    "IL2026:Members annotated with \'RequiresUnreferencedCodeAttribute\' require dynamic access otherwise can break functionality when trimming application code")]
public abstract class CliCommand<TArg> : ICliCommand 
    where TArg : CliCommandArgument, new() //not directly instantiated via TArg(), but is via Activator
{
    public ValueTask<ExitCode> InvokeAsync(object value)
    {
        TArg parsedArg = (TArg)value;

        Result pResult = parsedArg.BeforeExecution();

        if (pResult.IsOf<ExitCodeState>(out var ecs))
            return new(ecs.Code);

        if (pResult.IsOf<ExitCodeAndMessageState>(out var ecms))
        {
            Logger.Error(LogSource.App, ecms.Message);
            return new(ecms.Code);
        }

        if (pResult.TryUnwrapError(out var exc))
        {
            Logger.Error(LogSource.App, exc);
            return new(ExitCode.ArgumentParseFailed);
        }

        return ExecuteAsync(parsedArg);
    }

    protected abstract ValueTask<ExitCode> ExecuteAsync(TArg arg);
}

/// <summary>
///     Marker interface for reflective access.
/// </summary>
public interface ICliCommand
{
    public ValueTask<ExitCode> InvokeAsync(object args);
}