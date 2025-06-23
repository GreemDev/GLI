using GitLabCli.API.GitLab;
using GitLabCli.API.Helpers;
using GitLabCli.Helpers;
using Gommon;
using NGitLab.Models;

namespace GitLabCli.Commands.CreateReleaseFromGenericPackageFiles;

public class CreateReleaseFromGenericPackageFilesArgument : CliCommandArgument
{
    internal bool IsInit { get; private set; }
    
    public string PackageName { get; }
    public string PackageVersion { get; }

    public string ReleaseRef { get; }

    public string? ReleaseTitle { get; }
    public string? ReleaseBody { get; private set; }
    
    public CreateReleaseFromGenericPackageFilesArgument(Options options) : base(options)
    {
        PackageName = options.InputData.Split('|')[0];
        PackageVersion = options.InputData.Split('|')[1];
        ReleaseRef = options.InputData.Split('|')[2];

        try
        {
            ReleaseTitle = options.InputData.Split('|')[3];
        }
        catch
        {
            ReleaseTitle = null;
        }
        
        try
        {
            ReleaseBody = options.InputData.Split('|')[4];
        }
        catch
        {
            ReleaseBody = null;
        }
    }
    
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

        return p.FindOneAsync(it => it.Name == PackageName && it.Version == PackageVersion);
    }
    
    public async Task<ReleaseInfo?> CreateReleaseFromGenericPackagesAsync(Project project)
    {
        await InitIfNeededAsync(project);

        if (await FindMatchingPackageAsync(project) is not { } matchingPackage)
        {
            Logger.Error(LogSource.App,
                $"Could not create a release because a generic package matching name {PackageName}, version {PackageVersion} on project {Options.ProjectPath} wasn't found.");
            return null;
        }

        if (await matchingPackage.GetPackageFiles(Http, project).GetAllAsync() is not { } packageFiles)
        {
            Logger.Error(LogSource.App,
                $"Could not create a release because the request to get all package files for package matching name {PackageName}, version {PackageVersion} on project {Options.ProjectPath} failed.");
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