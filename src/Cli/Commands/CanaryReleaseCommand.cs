using CommandLine;
using ForgejoApiClient;
using gli.CommandLib;
using gli.Helpers;
using gli.REST.Forgejo;
using Gommon;

namespace gli.Commands;

[Verb("canary-release", 
    aliases: ["cr"],
    HelpText = "Creates links to the source code archives for a given canary release.")]
public partial class CanaryReleaseCommand : ForgejoCommand
{
    [Option('r', "release-ref", Required = true,
        HelpText = "The Git ref of the release. Usually a tag name.")]
    public string? ReleaseRef { get; set; } = null;

    protected override async ValueTask<ExitCode> InvokeAsync()
    {
        var release = await ForgejoClient.Repository.GetReleaseTagAsync(
            ProjectOwner, ProjectName, ReleaseRef!
            );

        if (release is null or { id: null })
        {
            Logger.Error(LogSource.App, $"Could not find a release with the tag {ReleaseRef} on project '{ProjectPath}' on '{ForgejoEndpoint}'. Missing permissions?");
            return ExitCode.ProjectNotFound;
        }

        await ForgejoClient.Repository.CreateReleaseAttachmentAsync(
            ProjectOwner, ProjectName, release.id.Value,
            external_url: $"https://git.ryujinx.app/projects/Ryubing/archive/Canary-{ReleaseRef}.zip",
            name: "Source Code (ZIP)");

        await ForgejoClient.Repository.CreateReleaseAttachmentAsync(
            ProjectOwner, ProjectName, release.id.Value,
            external_url: $"https://git.ryujinx.app/projects/Ryubing/archive/Canary-{ReleaseRef}.tar.gz",
            name: "Source Code (TAR.GZ)");

        Logger.Info(LogSource.App, $"Linked source code binaries for '{release.html_url}'.");
        return ExitCode.Normal;
    }
}