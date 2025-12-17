using System.Runtime;
using CommandLine;
using gli.Helpers;
using Gommon;

namespace gli.Commands;

/// <summary>
///     Marker interface for reflective access.
/// </summary>
public interface ICliCommand
{
    public CliCommandName Name { get; }
}

public abstract class CliCommand<TArg> : ICommandShimHolder, ICliCommand where TArg : CliCommandArgument
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
#if DEBUG
                Logger.Debug(LogSource.Cli,
                    $"> ./{Path.GetFileName(Environment.ProcessPath)} {Program.SearchString} {Parser.Default.FormatCommandLine(parsedResult.Value)}");
#endif
                return await ExecuteAsync(parsedResult.Value);
            default:
                // Should not be possible. Just here to shut up the compiler.
                throw new AmbiguousImplementationException();
        }
    }

    public CliCommandName Name { get; }

    protected abstract Task<ExitCode> ExecuteAsync(TArg arg);

    CommandShim ICommandShimHolder.Shim => new()
    {
        Name = Name,
        Execute = InvokeAsync
    };
}

public interface ICommandShimHolder
{
    internal CommandShim Shim { get; }
}

public struct CommandShim
{
    public required CliCommandName Name { get; init; }
    public required Func<string[], Task<ExitCode>>? Execute { get; init; }
}