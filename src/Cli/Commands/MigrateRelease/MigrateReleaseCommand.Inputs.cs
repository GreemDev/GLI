using System.Diagnostics;
using System.Net;
using CommandLine;
using ForgejoApiClient.Api;
using gli.Helpers;
using Gommon;
using NGitLab;
using NGitLab.Models;

namespace gli.Commands;

public partial class MigrateReleaseCommand
{
    [Option('h', "hide-source-code-archives", Required = false,
        HelpText =
            "Forgejo has the ability to let you hide the automatically generated source archives. This lets you set that.")]
    public bool HideSourceArchives { get; set; } = false;

    [Option('c', "canary-release", Required = false,
        HelpText =
            "Gives this release the 'Canary' treatment. If you don't know what this is don't bother setting it.")]
    public bool Canary { get; set; } = false;

    [Option('r', "release-ref", Required = true,
        HelpText = "The Git ref of the release. Usually a tag name.")]
    public string? ReleaseRef { get; set; } = null;

    [Option("forgejo-server-url", Required = false, Default = "https://git.greemdev.net/",
        HelpText = "The target GitLab instance to use.")]
    public string ForgejoEndpoint { get; set; } = null!;

    [Option("forgejo-access-token", Required = false, Default = null,
        HelpText =
            "https://git.greemdev.net/user/settings/applications/tokens/new | If a file next to the executable named '.accesstoken' exists, the contents of that file will be used here. An error will be thrown if that file does not exist and this argument is not provided.")]
    public string? ForgejoAccessToken { get; set; }

    [Option("forgejo-project", Required = true,
        HelpText = "The 'owner/project' you want to migrate the release to. For example, Ryubing/Ryujinx.")]
    public string ForgejoProjectPath { get; set; } = null!;

    public string ForgejoProjectOwner => ForgejoProjectPath.Split('/')[0];
    public string ForgejoProjectName => ForgejoProjectPath.Split('/')[1];

    public async Task<(Release Result, TimeSpan Elapsed)?> MigrateReleaseAsync(Project project)
    {
        ReleaseInfo release;

        TimeSpan elapsed = TimeSpan.Zero;

        try
        {
            release = GitLabClient.GetReleases(project.Id)[ReleaseRef];
        }
        catch (GitLabException gle) when (gle.StatusCode == HttpStatusCode.NotFound)
        {
            Logger.Error(LogSource.App, $"Release '{ReleaseRef}' was not found on '{project.NameWithNamespace}'.");
            return null;
        }

        Logger.Info(LogSource.App, $"Downloading all assets for {release.Name} (tag {release.TagName}).");

        Dictionary<string, Stream> downloadedAssets = new();

        foreach (var asset in release.Assets.Links.Where(x => x != null))
        {
            var sw = Stopwatch.StartNew();

            downloadedAssets[asset.Name] = await Http.GetAsync(asset.Url)
                .Then(x => x.Content.ReadAsStreamAsync());

            sw.Stop();

            elapsed += sw.Elapsed;

            Logger.Info(LogSource.App,
                $"Downloaded '{asset.Name}' in {sw.ElapsedMilliseconds}ms.");
        }

        var desc = release.Description;
        desc = desc.Replace("https://git.ryujinx.app/ryubing/ryujinx/-/compare/",
            "https://git.ryujinx.app/projects/Ryubing/compare/");

        var tempSw = Stopwatch.StartNew();

        var fjRelease = await ForgejoClient.Repository.CreateReleaseAsync(ForgejoProjectOwner, ForgejoProjectName,
            new CreateReleaseOption(
                name: release.Name,
                tag_name: ReleaseRef,
                body: desc,
                hide_archive_links: HideSourceArchives || Canary)
            );

        tempSw.Stop();
        elapsed += tempSw.Elapsed;

        if (fjRelease.id is null)
            return null;

        Logger.Info(LogSource.App,
            $"Created release '{fjRelease.name}' (tag '{fjRelease.tag_name}', ID {fjRelease.id}) in {tempSw.ElapsedMilliseconds}ms.");

        Logger.Info(LogSource.App, "Uploading all assets.");

        foreach (var (assetName, assetFile) in downloadedAssets)
        {
            var sw = Stopwatch.StartNew();

            var attachment = await ForgejoClient.Repository.CreateReleaseAttachmentAsync(
                ForgejoProjectOwner, ForgejoProjectName, fjRelease.id.Value,
                assetFile, name: assetName
            );

            sw.Stop();

            elapsed += sw.Elapsed;

            Logger.Info(LogSource.App,
                $"Uploaded '{assetName}' (attachment ID {attachment.id}) to release '{fjRelease.tag_name}' (ID {fjRelease.id}) in {sw.ElapsedMilliseconds}ms.");
        }

        if (Canary)
        {
            await ForgejoClient.Repository.CreateReleaseAttachmentAsync(
                ForgejoProjectOwner, ForgejoProjectName, fjRelease.id.Value,
                external_url: $"https://git.ryujinx.app/projects/Ryubing/archive/Canary-{ReleaseRef}.zip",
                name: "Source Code (ZIP)");

            await ForgejoClient.Repository.CreateReleaseAttachmentAsync(
                ForgejoProjectOwner, ForgejoProjectName, fjRelease.id.Value,
                external_url: $"https://git.ryujinx.app/projects/Ryubing/archive/Canary-{ReleaseRef}.tar.gz",
                name: "Source Code (TAR.GZ)");
        }

        downloadedAssets.ForEach(x => x.Value.Dispose());

        return (fjRelease, elapsed);
    }
}