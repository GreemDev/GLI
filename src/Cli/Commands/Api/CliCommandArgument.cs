using System.Diagnostics.CodeAnalysis;
using GitLabCli.API.GitLab;
using GitLabCli.API.Helpers;
using Gommon;
using NGitLab;

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

    [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract", Justification = "Special use case")]
    protected void InitHttp(TimeSpan? timeout = null)
    {
        if (Options is null) return;

        Http = GitLabRestApi.CreateHttpClient(Options.GitLabEndpoint, AccessToken, timeout);
    }

    public string FormatGitLabUrl(string subPath)
        => string.Concat(Options.GitLabEndpoint.TrimEnd('/'), "/", subPath);

    public IHttpClientProxy Http { get; private set; } = null!;

    public Options Options { get; protected init; }
    
    public GitLabClient CreateGitLabClient() 
        => new(Options.GitLabEndpoint, AccessToken);
}