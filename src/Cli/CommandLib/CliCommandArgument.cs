using gli.API.GitLab;
using gli.API.Helpers;
using Gommon;
using NGitLab;

namespace gli.Commands;

public sealed class DefaultCliCommandArgument : CliCommandArgument;

public abstract class CliCommandArgument : Options
{
    private static string ReadAccessTokenFromFile()
    {
        var fp = new FilePath(Environment.CurrentDirectory) / ".accesstoken";
        if (!fp.ExistsAsFile)
            throw new FileNotFoundException(
                    "Could not find an .accesstoken file. Either provide the argument (--access-token) or create the file.");

        return fp.ReadAllText();
    }

    public virtual TimeSpan? HttpRequestTimeout => null;

    /// <remarks>ALWAYS call the base implementation when overriding! Respect the returned result so long as the derived type needs to authenticate with GitLab.</remarks>
    internal virtual Result BeforeExecution()
    {
        Http = GitLabRestApi.CreateHttpClient(GitLabEndpoint, AccessToken!, HttpRequestTimeout);
        try
        {
            AccessToken ??= ReadAccessTokenFromFile();
        }
        catch (FileNotFoundException fnfe)
        {
            return Result.Failure(new MessageError(fnfe.Message));
        }

        return Result.Success;
    }

    public string FormatGitLabUrl(string subPath)
        => string.Concat(GitLabEndpoint.TrimEnd('/'), "/", subPath);

    public IHttpClientProxy Http { get; private set; } = null!;
    
    public GitLabClient CreateGitLabClient() 
        => new(GitLabEndpoint, AccessToken);
}