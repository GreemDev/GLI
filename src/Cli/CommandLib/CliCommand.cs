using System.Diagnostics.CodeAnalysis;
using CommandLine;
using gli.Helpers;
using Gommon;

namespace gli.CommandLib;

[SuppressMessage("Trimming",
    "IL2026:Members annotated with \'RequiresUnreferencedCodeAttribute\' require dynamic access otherwise can break functionality when trimming application code")]
public abstract class CliCommand<TArg> : ICliCommand where TArg : CliCommandArgument
{
    protected CliCommand(CliCommandName name)
    {
        Name = name;
    }

    public ValueTask<ExitCode> InvokeAsync(string[] args)
    {
        ParserResult<TArg> parserResult;
        try
        {
            parserResult = Parser.CustomDefault.ParseArguments<TArg>(args);
        }
        catch (InvalidOperationException ioe)
        {
            if (ioe.TargetSite?.Name is "ThrowMoreThanOneMatchException")
            {
                Logger.Info(LogSource.App, $"{Name} has options with names that conflict with the type it's derived from. Check the implementation.");
                return new(ExitCode.OperationFailure);
            }

            throw;
        }

        if (parserResult is NotParsed<TArg> notParsedResult)
        {
            Logger.WriteToFile = false;

            notParsedResult.Errors.ForEach(err => Logger.Error(LogSource.Cli, $" - {err.Tag}"));

            return new(ExitCode.ArgumentParseFailed);
        }

        TArg parsedArg = parserResult is Parsed<TArg> parsed ? parsed.Value : null!;

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

    public CliCommandName Name { get; }

    protected abstract ValueTask<ExitCode> ExecuteAsync(TArg arg);
}

/// <summary>
///     Marker interface for reflective access.
/// </summary>
public interface ICliCommand
{
    public CliCommandName Name { get; }
    public ValueTask<ExitCode> InvokeAsync(string[] args);
}