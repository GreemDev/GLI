using System.Diagnostics.CodeAnalysis;
using GitLabCli.API.GitLab;
using GitLabCli.API.Helpers;
using Gommon;
using NGitLab;

namespace GitLabCli.Commands;

public abstract class CliCommandArgument : Options
{
    protected CliCommandArgument()
    {
        AccessToken ??= ReadAccessTokenFromFile();
    }

    protected static string ReadAccessTokenFromFile()
    {
        var fp = new FilePath(Environment.CurrentDirectory) / ".accesstoken";
        if (!fp.ExistsAsFile)
            throw new FileNotFoundException(
                    "Could not find an .accesstoken file. Either provide the argument or create the file.");

        return fp.ReadAllText();
    }

    [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract", Justification = "Special use case")]
    internal void InitHttp(TimeSpan? timeout = null)
    {
        Http = GitLabRestApi.CreateHttpClient(GitLabEndpoint, AccessToken, timeout);
    }

    public string FormatGitLabUrl(string subPath)
        => string.Concat(GitLabEndpoint.TrimEnd('/'), "/", subPath);

    public IHttpClientProxy Http { get; private set; } = null!;
    
    public GitLabClient CreateGitLabClient() 
        => new(GitLabEndpoint, AccessToken);
}