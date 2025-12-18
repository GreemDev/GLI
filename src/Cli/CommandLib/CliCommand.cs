using System.Runtime;
using CommandLine;
using gli.Helpers;
using Gommon;

namespace gli.CommandLib;

public abstract class CliCommand<TArg> : ICliCommand where TArg : CliCommandArgument
{
    protected CliCommand(CliCommandName name)
    {
        Name = name;
    }

    public async Task<ExitCode> InvokeAsync(string[] args)
    {
        var parserResult = Parser.CustomDefault.ParseArguments<TArg>(args);

        if (parserResult is NotParsed<TArg> notParsedResult)
        {
            Logger.WriteToFile = false;

            notParsedResult.Errors.ForEach(err => Logger.Error(LogSource.Cli, $" - {err.Tag}"));

            return ExitCode.ArgumentParseFailed;
        }

        TArg parsedArg = parserResult is Parsed<TArg> parsed ? parsed.Value : null!;

        Result pResult = parsedArg.BeforeExecution();

        if (pResult.IsOf<ExitCodeState>(out var ecs))
            return ecs.Code;

        if (pResult.IsOf<ExitCodeAndMessageState>(out var ecms))
        {
            Logger.Error(LogSource.App, ecms.Message);
            return ecms.Code;
        }

        if (pResult.TryUnwrapError(out var exc))
        {
            Logger.Error(LogSource.App, exc);
            return ExitCode.ArgumentParseFailed;
        }

        return await ExecuteAsync(parsedArg);
    }

    public CliCommandName Name { get; }

    protected abstract Task<ExitCode> ExecuteAsync(TArg arg);
}

/// <summary>
///     Marker interface for reflective access.
/// </summary>
public interface ICliCommand
{
    public CliCommandName Name { get; }
    public Task<ExitCode> InvokeAsync(string[] args);
}