using Gommon;
using NGitLab;
using NGitLab.Models;

namespace GitLabCli.Commands;

public abstract class CliCommandArgument
{
    protected CliCommandArgument(Options options)
    {
        Options = options;
        AccessToken = options.AccessToken ?? ReadAccessTokenFromFile();
    }

    public string AccessToken { get; }

    private static string ReadAccessTokenFromFile()
    {
        var fp = new FilePath(Environment.CurrentDirectory) / ".accesstoken";
        if (!fp.ExistsAsFile)
            throw new FileNotFoundException(
                    "Could not find an .accesstoken file. Either provide the argument or create the file.");

        return fp.ReadAllText();
    }

    public Options Options { get; }

    public IRepositoryClient? GetRepoClient() => CreateGitLabClient().GetRepository(new ProjectId(Options.ProjectPath));
    
    public GitLabClient CreateGitLabClient() 
        => new(Options.GitLabEndpoint, AccessToken);
}