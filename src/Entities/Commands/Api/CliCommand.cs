using GitLabCli.Entities.Cli;
using NGitLab;

namespace GitLabCli.Entities.Commands;

public abstract class CliCommand
{
    protected CliCommand(CliCommandName name) => Name = name;

    public readonly CliCommandName Name;
    
    public abstract Task ExecuteAsync(CliCommandArgument arg);

    protected GitLabClient CreateGitLabClient(Options options) 
        => new(options.GitLabEndpoint, options.AccessToken);
}