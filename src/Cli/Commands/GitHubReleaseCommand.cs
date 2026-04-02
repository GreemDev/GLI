using CommandLine;
using gli.CommandLib;
using gli.Helpers;
using gli.REST.GitHub;
using Gommon;

namespace gli.Commands;

[Verb("github-release", aliases: ["ghr", "dl-release"],
    HelpText =
        "Retrieves the latest, or a specific, release from a repository, and lets you download an asset without auth.")]
public class GitHubReleaseCommand : GitHubCommand
{
    protected override bool NeedsAuthorization => false;
    public override TimeSpan? HttpRequestTimeout => TimeSpan.FromMinutes(10); //accommodate shitass internet

    [Value(index: 0, Required = false, Default = "latest", HelpText =
        "Get a release matching the specified tag. If this is not provided, the latest release is retrieved.")]
    public string DesiredTag { get; set; } = "latest";

    [Option('p', "pattern", Required = true,
        HelpText = "Download files matching the specified glob pattern.")]
    public string FilePattern { get; set; } = null!;

    [Option('O', "output",
        HelpText =
            "The name of the output file. Defaults to the file's asset name. Will cause an error if the file pattern matches multiple files.")]
    public string? OutputPathName { get; set; }

    protected override async ValueTask<ExitCode> InvokeAsync()
    {
        var release = await (DesiredTag is null or "latest"
            ? GitHubApi.GetLatestReleaseAsync(Http, Repository)
            : GitHubApi.GetReleaseAsync(Http, Repository, DesiredTag));

        if (release is null)
            return ExitCode.ObjectNotFound;

        if (Traversal.Match(release.Assets, Traversal.CreateMatcher(FilePattern))
            .TryUnwrap(out var assets, out var err))
        {
            Logger.Info(LogSource.App,
                $"Downloading {assets!.Count} asset{(assets.Count != 1 ? 's' : string.Empty)} from release " +
                $"'{release.Name}' on repository '{Repository}'...");

            if (DownloadAssets(assets).TryUnwrap(out var task, out err))
                await task!;
            else
            {
                Logger.Error(LogSource.App, err!.Message);
                return ExitCode.OperationFailure;
            }
        }
        else
        {
            Logger.Error(LogSource.App, err!);
            return ExitCode.OperationFailure;
        }

        return ExitCode.Normal;
    }

    private Return<Task> DownloadAssets(List<ReleaseData.AssetData> assets)
    {
        if (assets.Count is 1)
        {
            return download(Http, assets[0], OutputPathName);
        }

        if (OutputPathName != null)
        {
            return Return<Task>.Failure(new MessageError("Cannot write multiple files to a single output path."));
        }

        return Task.WhenAll(assets.Select(x => download(Http, x)));

        static async Task download(IHttpClientProxy http, ReleaseData.AssetData asset, string? outputName = null)
        {
            Logger.Info(LogSource.App, $"Downloading '{asset.Name}'{
                (outputName != null ? $"to '{outputName}'" : string.Empty)
            }...");

            await using var fs = new FileStream(outputName ?? asset.Name, FileMode.Create);

            await http.GetAsync(asset.DownloadUrl)
                .Then(x => x.Content.ReadAsStreamAsync())
                .ThenUse(x => x.CopyToAsync(fs));
        }
    }
}