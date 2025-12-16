using System.Diagnostics.CodeAnalysis;
using gli.API.GitLab;
using gli.API.Helpers;
using Gommon;
using NGitLab;

namespace gli.Commands;

public sealed class DefaultCliCommandArgument : CliCommandArgument;

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
    internal virtual void InitHttp(TimeSpan? timeout = null)
    {
        Http = GitLabRestApi.CreateHttpClient(GitLabEndpoint, AccessToken, timeout);
    }

    public string FormatGitLabUrl(string subPath)
        => string.Concat(GitLabEndpoint.TrimEnd('/'), "/", subPath);

    public IHttpClientProxy Http { get; private set; } = null!;
    
    public GitLabClient CreateGitLabClient() 
        => new(GitLabEndpoint, AccessToken);
}