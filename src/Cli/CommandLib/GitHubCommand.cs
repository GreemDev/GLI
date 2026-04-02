using CommandLine;
using gli.Helpers;
using gli.REST.GitHub;
using Gommon;

namespace gli.CommandLib;

public abstract class GitHubCommand : Command
{
    protected abstract bool NeedsAuthorization { get; }

    [Option('T', "access-token", Required = false, Default = null,
        HelpText =
            "https://github.com/settings/tokens | If a file next to the executable named '.ghaccesstoken' exists, the contents of that file will be used here. An error will be thrown if that file does not exist and this argument is not provided.")]
    public string? AccessToken { get; set; }

    [Option('R', "repository", Required = true,
        HelpText = "The 'owner/project' you are requesting. For example, GreemDev/GLI.")]
    public string Repository { get; set; } = null!;

    public IHttpClientProxy Http { get; private set; } = null!;

    /// <remarks>ALWAYS call the base implementation when overriding! Respect the returned result so long as the derived type needs to authenticate with GitHub.</remarks>
    protected override Result BeforeExecution()
    {
        try
        {
            AccessToken ??= ReadAccessTokenFromFile();
        }
        catch (FileNotFoundException fnfe)
        {
            if (NeedsAuthorization)
                return Result.MessageFailure(fnfe.Message);
        }

        Http = GitHubApi.CreateHttpClient(AccessToken, HttpRequestTimeout);

        return Result.Success;
    }

    private static string ReadAccessTokenFromFile()
    {
        var fp = new FilePath(Environment.CurrentDirectory) / ".ghaccesstoken";
        if (!fp.ExistsAsFile)
            throw new FileNotFoundException(
                "Could not find a .ghaccesstoken file. Either provide the argument (--access-token) or create the file.");

        var lines = fp.ReadAllLines();

        if (lines == null || lines.Length == 0)
        {
            throw new FormatException(
                ".ghaccesstoken file could not be read or did not contain any content");
        }

        return lines[0];
    }
}