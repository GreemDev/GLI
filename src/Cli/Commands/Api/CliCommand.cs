using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime;
using CommandLine;
using gli.Helpers;
using Gommon;

namespace gli.Commands;

public abstract class CliCommand<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TArg>
    where TArg : CliCommandArgument
{
    protected CliCommand(CliCommandName name) => Name = name;

    public readonly CliCommandName Name;

    public abstract Task<ExitCode> ExecuteAsync(TArg arg);

    // ReSharper disable once UnusedMember.Global
    internal CommandShim CreateShim()
        => new()
        {
            Name = Name,
            Execute = async args =>
            {
                var parserResult = Parser.Default.ParseArguments<TArg>(args);

                if (parserResult is NotParsed<TArg> notParsedResult)
                {
                    Logger.WriteToFile = false;
                    Logger.Error(LogSource.Cli, $"Error parsing arguments for {Enum.GetName(Name)}:");

                    notParsedResult.Errors.ForEach(err => Logger.Error(LogSource.Cli, $" - {err.Tag}"));

                    return ExitCode.ArgumentParseFailed;
                }

                if (parserResult is Parsed<TArg> parsedResult)
                {
                    parsedResult.Value.InitHttp();
                    return await ExecuteAsync(parsedResult.Value);
                }

                // Should not be possible. Just here to shut up the compiler.
                throw new AmbiguousImplementationException();
            }
        };
}

internal struct CommandShim
{
    public required CliCommandName Name { get; init; }
    public required Func<string[], Task<ExitCode>> Execute { get; init; }
}

public enum CliCommandName
{
    CreateTag,
    BulkUploadGenericPackage,
    UploadGenericPackage,
    SendUpdateMessage,
    CreateReleaseFromGenericPackageFiles,
    GetNextVersion
}