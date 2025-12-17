using CommandLine;
using gli.API.GitLab;
using gli.API.Helpers;
using gli.Helpers;
using Gommon;
using NGitLab.Models;

namespace gli.Commands.CreateReleaseFromGenericPackageFiles;

public class CreateReleaseFromGenericPackageFilesArgument : GitLabCliCommandArgument
{
    internal bool IsInit { get; private set; }

    [Option('n', "package-name", Required = true, HelpText = "The name of the generic package to list the items of.")]
    public string PackageName { get; set; } = null!;

    [Option('v', "package-version", Required = true, HelpText = "The desired version of the generic package.")]
    public string PackageVersion { get; set; } = null!;

    [Option('r', "release-ref", Required = false,
        HelpText =
            "The Git ref this release should be tied to. If not passed, creates a release and tag at the same time based on the main branch.")]
    public string? ReleaseRef { get; set; } = null;

    [Option('t', "release-title", Required = false, HelpText = "Title of the release in the GitLab UI.")]
    public string? ReleaseTitle { get; set; } = null;

    [Option('b', "release-body", Required = false, HelpText = "Body of the release in the GitLab UI.")]
    public string? ReleaseBody { get; set; }

    public async Task InitIfNeededAsync(Project project)
    {
        if (ReleaseBody is null || IsInit) return;

        if (ReleaseBody.StartsWithIgnoreCase("rf:"))
            ReleaseBody = await File.ReadAllTextAsync(ReleaseBody[3..]);

        if (ReleaseBody.StartsWithIgnoreCase("msd:"))
        {
            var milestoneTitle = ReleaseBody[4..];

            if (await GitLabRestApi.GetMilestoneByTitleAsync(Http, project, milestoneTitle) is { } milestone)
                ReleaseBody = milestone.Description;
        }

        IsInit = true;
    }

    public Task<GetProjectPackagesItem?> FindMatchingPackageAsync(Project project)
    {
        var p = PaginatedEndpoint<GetProjectPackagesItem>.Builder(Http)
            .WithBaseUrl($"api/v4/projects/{project.Id}/packages")
            .WithJsonContentParser(SerializerContexts.Default.IEnumerableGetProjectPackagesItem)
            .WithPerPageCount(100)
            .WithQueryStringParameters(
                QueryParameters.Sort("desc"),
                QueryParameters.OrderBy("created_at"),
                ("package_type", "generic")
            ).Build();

        return p.FindOneAsync(
            predicate: it => it.Name == PackageName && it.Version == PackageVersion,
            onNonSuccess: _ => Logger.Error(LogSource.App, "Target project has the package registry disabled.")
        );
    }

    public async Task<ReleaseInfo?> CreateReleaseFromGenericPackagesAsync(Project project)
    {
        await InitIfNeededAsync(project);

        if (await FindMatchingPackageAsync(project) is not { } matchingPackage)
        {
            Logger.Error(LogSource.App,
                $"Could not create a release because a generic package matching name {PackageName}, version {PackageVersion} on project {ProjectPath} wasn't found.");
            return null;
        }

        var packageFiles = await matchingPackage.GetPackageFiles(Http, project)
            .GetAllAsync(
                onNonSuccess: _ => Logger.Error(LogSource.App, "Target project has the package registry disabled."));

        if (packageFiles is null)
        {
            Logger.Error(LogSource.App,
                $"Could not create a release because the request to get all package files for package matching name {PackageName}, version {PackageVersion} on project {ProjectPath} failed.");
            return null;
        }

        var gitlabAssetLinks = packageFiles.Select(x => new ReleaseLink
        {
            Name = x.Name,
            LinkType = ReleaseLinkType.Package,
            Url = FormatGitLabUrl(
                $"api/v4/projects/{project.Id}/packages/generic/{PackageName}/{PackageVersion}/{x.Name}")
        }).ToArray();

        try
        {
            return await CreateGitLabClient().GetReleases(project.Id).CreateAsync(new ReleaseCreate
            {
                TagName = PackageVersion,
                Ref = ReleaseRef.EqualsAnyIgnoreCase("null") ? null : ReleaseRef,
                Name = ReleaseTitle ?? PackageVersion,
                Description = ReleaseBody,
                Assets = new ReleaseAssetsInfo
                {
                    Count = gitlabAssetLinks.Length,
                    Links = gitlabAssetLinks
                }
            });
        }
        catch (Exception e)
        {
            Logger.Error(e);
            return null;
        }
    }
}