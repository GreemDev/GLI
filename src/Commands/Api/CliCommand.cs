using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace GitLabCli.Commands;

public abstract class CliCommand<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TArg> where TArg : CliCommandArgument
{
    protected CliCommand(CliCommandName name) => Name = name;

    public readonly CliCommandName Name;
    
    public abstract Task<ExitCode> ExecuteAsync(TArg arg);

    // ReSharper disable once UnusedMember.Global
    internal CommandShim CreateShim()
        => new()
        {
            Name = Name,
            Execute = options => ExecuteAsync(
                (TArg)typeof(TArg)
                    .GetConstructor(BindingFlags.Public | BindingFlags.Instance, [typeof(Options)])!
                    .Invoke([options])
                )
        };
}

internal struct CommandShim
{
    public required CliCommandName Name { get; init; }
    public required Func<Options, Task<ExitCode>> Execute { get; init; }
}

public enum CliCommandName
{
    CreateTag,
    BulkUploadGenericPackage,
    UploadGenericPackage,
    SendUpdateMessage,
    CreateReleaseFromGenericPackageFiles
}