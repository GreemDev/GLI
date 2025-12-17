using System.Runtime;
using CommandLine;
using gli.Helpers;
using Gommon;

namespace gli.Commands;

public abstract class CliCommand<TArg> : ICommandShimHolder where TArg : CliCommandArgument
{
    protected CliCommand(CliCommandName name)
    {
        Name = name;
    }

    public async Task<ExitCode> InvokeAsync(string[] args)
    {
        var parserResult = Parser.Default.ParseArguments<TArg>(args);

        switch (parserResult)
        {
            case NotParsed<TArg> notParsedResult:
                Logger.WriteToFile = false;
                Logger.Error(LogSource.Cli, $"Error parsing arguments for {Enum.GetName(Name)}:");

                notParsedResult.Errors.ForEach(err => Logger.Error(LogSource.Cli, $" - {err.Tag}"));

                return ExitCode.ArgumentParseFailed;
            case Parsed<TArg> parsedResult:
                parsedResult.Value.BeforeExecution();
                return await ExecuteAsync(parsedResult.Value);
            default:
                // Should not be possible. Just here to shut up the compiler.
                throw new AmbiguousImplementationException();
        }
    }

    public readonly CliCommandName Name;

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
    public required Func<string[], Task<ExitCode>> Execute { get; init; }
}