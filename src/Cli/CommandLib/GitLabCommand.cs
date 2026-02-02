using CommandLine;
using gli.Helpers;
using gli.REST.GitLab;
using Gommon;

namespace gli.CommandLib;

public abstract class GitLabCommand : Command
{
    [Option('S', "server-url", Required = false, Default = "https://git.ryujinx.app",
        HelpText = "The target GitLab instance to use.")]
    public string GitLabEndpoint { get; set; } = null!;

    [Option('T', "access-token", Required = false, Default = null,
        HelpText =
            "https://git.ryujinx.app/-/user_settings/personal_access_tokens | If a file next to the executable named '.accesstoken' exists, the contents of that file will be used here. An error will be thrown if that file does not exist and this argument is not provided.")]
    public string? AccessToken { get; set; }

    [Option('P', "project", Required = true,
        HelpText = "The 'owner/project' you are requesting. For example, ryubing/ryujinx.")]
    public string ProjectPath { get; set; } = null!;

    public string FormatGitLabUrl(string subPath)
        => string.Concat(GitLabEndpoint.TrimEnd('/'), "/", subPath);

    public IHttpClientProxy Http { get; private set; } = null!;

    /// <remarks>ALWAYS call the base implementation when overriding! Respect the returned result so long as the derived type needs to authenticate with GitLab.</remarks>
    protected override Result BeforeExecution()
    {
        try
        {
            AccessToken ??= ReadAccessTokenFromFile();
        }
        catch (FileNotFoundException fnfe)
        {
            return Result.MessageFailure(fnfe.Message);
        }

        Http = GitLabApi.CreateHttpClient(GitLabEndpoint, AccessToken, HttpRequestTimeout);

        return Result.Success;
    }

    private static string ReadAccessTokenFromFile()
    {
        var fp = new FilePath(Environment.CurrentDirectory) / ".accesstoken";
        if (!fp.ExistsAsFile)
            throw new FileNotFoundException(
                "Could not find an .accesstoken file. Either provide the argument (--access-token) or create the file.");

        return fp.ReadAllText();
    }
}