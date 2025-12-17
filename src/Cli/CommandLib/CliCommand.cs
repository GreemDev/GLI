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

        switch (parserResult)
        {
            case NotParsed<TArg> notParsedResult:
                Logger.WriteToFile = false;

                notParsedResult.Errors.ForEach(err => Logger.Error(LogSource.Cli, $" - {err.Tag}"));

                return ExitCode.ArgumentParseFailed;
            case Parsed<TArg> parsedResult:
                Result preconditionResult = parsedResult.Value.BeforeExecution();
                if (preconditionResult.IsOf<MessageError>(out var me))
                {
                    Logger.Error(LogSource.App, me.Content);
                    return ExitCode.ArgumentParseFailed;
                }

                return await ExecuteAsync(parsedResult.Value);
            default:
                // Should not be possible. Just here to shut up the compiler.
                throw new AmbiguousImplementationException();
        }
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