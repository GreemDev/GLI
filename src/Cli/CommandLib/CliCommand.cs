using gli.Helpers;
using Gommon;

namespace gli.CommandLib;

/// <summary>
///     Marker interface for reflective access.
/// </summary>
public interface ICliCommand
{
    public ValueTask<ExitCode> InvokeAsync();

    public static ValueTask<ExitCode> InvokeAsync(CliCommand command)
    {
        Result pResult = command.BeforeExecution();

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

        return command.InvokeAsync();
    }
}