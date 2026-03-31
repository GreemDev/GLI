using CommandLine;
using ForgejoApiClient;
using gli.Helpers;
using gli.REST.Forgejo;
using Gommon;

namespace gli.CommandLib;

public abstract class ForgejoCommand : Command
{
    [Option('S', "server-url", Required = false, Default = "https://git.greemdev.net/",
        HelpText = "The target GitLab instance to use.")]
    public string ForgejoEndpoint { get; set; } = null!;

    [Option('T', "access-token", Required = false, Default = null,
        HelpText =
            "https://git.greemdev.net/user/settings/applications/tokens/new | If a file next to the executable named '.accesstoken' exists, the contents of that file will be used here. An error will be thrown if that file does not exist and this argument is not provided.")]
    public string? AccessToken { get; set; }

    [Option('P', "project", Required = true,
        HelpText = "The 'owner/project' you are requesting. For example, Ryubing/Ryujinx.")]
    public string ProjectPath { get; set; } = null!;

    public string ProjectOwner => ProjectPath.Split('/')[0];
    public string ProjectName => ProjectPath.Split('/')[1];

    public string FormatForgejoUrl(string subPath)
        => string.Concat(ForgejoEndpoint.TrimEnd('/'), "/", subPath);

    public IHttpClientProxy Http { get; private set; } = null!;

    public ForgejoClient ForgejoClient { get; private set; } = null!;

    /// <remarks>ALWAYS call the base implementation when overriding! Respect the returned result so long as the derived type needs to authenticate with Forgejo.</remarks>
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

        Http = ForgejoApi.CreateHttpClient(ForgejoEndpoint, AccessToken, HttpRequestTimeout);
        try
        {
            ForgejoClient = new ForgejoClient(new Uri(ForgejoEndpoint), AccessToken);
        }
        catch (FormatException fe)
        {
            return Result.MessageFailure(fe.Message);
        }

        return Result.Success;
    }

    private static string ReadAccessTokenFromFile()
    {
        var fp = new FilePath(Environment.CurrentDirectory) / ".fjaccesstoken";
        if (!fp.ExistsAsFile)
            throw new FileNotFoundException(
                "Could not find a .fjaccesstoken file. Either provide the argument (--access-token) or create the file.");

        var lines = fp.ReadAllLines();

        if (lines == null || lines.Length == 0)
        {
            throw new FormatException(
                ".fjaccesstoken file could not be read or did not contain any content");
        }

        return lines[0];
    }
}