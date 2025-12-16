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

    public virtual TimeSpan? HttpRequestTimeout { get; } = null;
    
    /// <remarks>ALWAYS call the base implementation when overriding! If you don't, <see cref="Http"/> will be null and any HTTP requests will error with null reference exceptions.</remarks>
    internal virtual void BeforeExecution()
    {
        Http = GitLabRestApi.CreateHttpClient(GitLabEndpoint, AccessToken!, HttpRequestTimeout);
    }

    public string FormatGitLabUrl(string subPath)
        => string.Concat(GitLabEndpoint.TrimEnd('/'), "/", subPath);

    public IHttpClientProxy Http { get; private set; } = null!;
    
    public GitLabClient CreateGitLabClient() 
        => new(GitLabEndpoint, AccessToken);
}