using GitLabCli.API.GitLab;
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

    public string FormatGitLabUrl(string subPath)
        => string.Concat(Options.GitLabEndpoint.TrimEnd('/'), "/", subPath);

    public HttpClient Http { get; private set; } = null!;

    public Options Options { get; protected init; }
    
    public GitLabClient CreateGitLabClient() 
        => new(Options.GitLabEndpoint, AccessToken);
}