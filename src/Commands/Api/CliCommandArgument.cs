using GitLabCli.API;
using Gommon;
using NGitLab;
using NGitLab.Models;

namespace GitLabCli.Commands;

public abstract class CliCommandArgument
{
    protected CliCommandArgument(Options options)
    {
        Options = options;
        AccessToken = options?.AccessToken ?? ReadAccessTokenFromFile();
        InitHttp();
    }

    public string AccessToken { get; protected init; }

    protected static string ReadAccessTokenFromFile()
    {
        var fp = new FilePath(Environment.CurrentDirectory) / ".accesstoken";
        if (!fp.ExistsAsFile)
            throw new FileNotFoundException(
                    "Could not find an .accesstoken file. Either provide the argument or create the file.");

        return fp.ReadAllText();
    }

    protected void InitHttp()
    {
        Http = GitLabRestApi.CreateHttpClient(Options.GitLabEndpoint, AccessToken);
    }

    public HttpClient Http { get; private set; } = null!;

    public Options Options { get; protected init; }

    public IRepositoryClient? GetRepoClient() => CreateGitLabClient().GetRepository(new ProjectId(Options.ProjectPath));
    
    public GitLabClient CreateGitLabClient() 
        => new(Options.GitLabEndpoint, AccessToken);
}