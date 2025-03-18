using GitLabCli.Entities.Cli;
using NGitLab;

namespace GitLabCli.Entities.Commands;

public abstract class CliCommandArgument
{
    protected CliCommandArgument(Options options) => Options = options;

    public Options Options { get; }
    
    public GitLabClient CreateGitLabClient() 
        => new(Options.GitLabEndpoint, Options.AccessToken);
}