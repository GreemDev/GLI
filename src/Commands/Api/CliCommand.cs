﻿namespace GitLabCli.Commands;

public abstract class CliCommand<TArg> where TArg : CliCommandArgument
{
    protected CliCommand(CliCommandName name) => Name = name;

    public readonly CliCommandName Name;
    
    public abstract Task<ExitCode> ExecuteAsync(TArg arg);

    protected abstract TArg CreateArg(Options options);

    // ReSharper disable once UnusedMember.Global
    internal CommandShim CreateShim()
        => new()
        {
            Name = Name,
            Execute = options => ExecuteAsync(CreateArg(options))
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