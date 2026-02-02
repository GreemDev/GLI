using CommandLine;
using gli.CommandLib;
using gli.Helpers;
using gli.REST.GitLab;

namespace gli.Commands;

[Verb("create-release-from-generic-package-files", 
    aliases: ["crfgpf"],
    HelpText = "Creates a release linking to the package files of a given GitLab Generic Package Registry package. Allows setting release title/body.")]
public partial class CreateReleaseFromGenericPackageFilesCommand : GitLabCommand
{
    protected override async ValueTask<ExitCode> InvokeAsync()
    {
        var project = await GitLabApi.GetProjectAsync(Http, ProjectPath);
        if (project is null)
        {
            Logger.Error(LogSource.App, $"Could not find the project '{ProjectPath}' on '{GitLabEndpoint}'.");
            return ExitCode.ProjectNotFound;
        }

        if (await CreateReleaseFromGenericPackagesAsync(project) is not { } releaseInfo)
            return ExitCode.ObjectNotFound;

        Logger.Info(LogSource.App, $"Release created at '{releaseInfo.Links.Self}'.");
        return ExitCode.Normal;
    }
}