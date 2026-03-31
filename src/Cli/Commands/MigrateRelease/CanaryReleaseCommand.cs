using CommandLine;
using ForgejoApiClient;
using gli.CommandLib;
using gli.Helpers;
using gli.REST.Forgejo;
using Gommon;

namespace gli.Commands;

[Verb("migrate-release", 
    aliases: ["mr"],
    HelpText = "Migrates a release and its assets from a GitLab project to a Forgejo repository.")]
public partial class MigrateReleaseCommand : GitLabCommand
{
    public IHttpClientProxy ForgejoHttp { get; private set; } = null!;
    public ForgejoClient ForgejoClient { get; private set; } = null!;
    
    protected override async ValueTask<ExitCode> InvokeAsync()
    {
        var project = await GitLabClient.Projects.GetByNamespacedPathAsync(ProjectPath);
        if (project is null)
        {
            Logger.Error(LogSource.App, $"Could not find the project '{ProjectPath}' on '{GitLabEndpoint}'.");
            return ExitCode.ProjectNotFound;
        }

        if (await MigrateReleaseAsync(project) is not { } release)
            return ExitCode.ObjectNotFound;

        Logger.Info(LogSource.App, $"Release created at '{release.Result.html_url}' in {release.Elapsed.TotalSeconds}s.");
        return ExitCode.Normal;
    }

    protected override Result BeforeExecution()
    {
        try
        {
            ForgejoAccessToken ??= ReadAccessTokenFromFile();
        }
        catch (FileNotFoundException fnfe)
        {
            return Result.MessageFailure(fnfe.Message);
        }

        ForgejoHttp = ForgejoApi.CreateHttpClient(ForgejoEndpoint, ForgejoAccessToken, HttpRequestTimeout);
        try
        {
            ForgejoClient = new ForgejoClient(new Uri(ForgejoEndpoint), ForgejoAccessToken);
        }
        catch (FormatException fe)
        {
            return Result.MessageFailure(fe.Message);
        }

        return base.BeforeExecution();
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