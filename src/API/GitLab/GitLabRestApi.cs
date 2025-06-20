using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using GitLabCli.Commands.BulkUploadGenericPackage;
using GitLabCli.Commands.CreateReleaseFromGenericPackageFiles;
using GitLabCli.Commands.SendUpdateMessage;
using GitLabCli.Commands.UploadGenericPackage;
using GitLabCli.Helpers;
using Gommon;
using NGitLab.Models;

namespace GitLabCli.API.GitLab;

public static partial class GitLabRestApi
{
    public static HttpClient CreateHttpClient(string host, string accessToken)
    {
        return new HttpClient
        {
            BaseAddress = new Uri(host),
            DefaultRequestHeaders =
            {
                UserAgent = { new ProductInfoHeaderValue("GitLabCli", "1.0.0") },
                Authorization = AuthenticationHeaderValue.Parse($"Bearer {accessToken}")
            }
        };
    }

    public static async Task<MilestoneItem?> GetMilestoneByTitleAsync(HttpClient httpClient, Project project,
        string title)
    {
        var resp = await httpClient
            .GetAsync($"api/v4/projects/{project.Id}/milestones?title={title}&include_ancestors=true");

        if (resp.StatusCode == HttpStatusCode.Forbidden)
        {
            Logger.Error(LogSource.App, $"'{project.NameWithNamespace}' has issues disabled.");
            return null;
        }

        var milestones = JsonSerializer.Deserialize(await resp.Content.ReadAsStringAsync(),
            MilestoneItemSerializerContext.Default.MilestoneItemArray);

        if (milestones is null || milestones.Length is 0)
        {
            Logger.Error(LogSource.App,
                $"Project '{project.NameWithNamespace}' and its parents did not have a milestone matching title '{title}'.");
            return null;
        }

        if (milestones.Length > 1)
        {
            Logger.Error(LogSource.App,
                $"Project '{project.NameWithNamespace}' had multiple milestones (including group milestones) matching title '{title}'.");
            Logger.Error(LogSource.App, "Using the one with the largest description content.");
            return milestones.OrderByDescending(m => m.Description.Length).First();
        }

        return milestones.First();
    }

    public static Task<GitLabReleaseJsonResponse?> GetLatestReleaseAsync(HttpClient httpClient, Project project)
        => GetReleaseAsync(httpClient, project, "permalink/latest");

    public static async Task<GitLabReleaseJsonResponse?> GetReleaseAsync(HttpClient httpClient,
        Project project, string tagName)
    {
        var resp = await httpClient.GetAsync($"api/v4/projects/{project.Id}/releases/{tagName}");

        if (resp.StatusCode == HttpStatusCode.Forbidden)
        {
            Logger.Error(LogSource.App, $"'{project.NameWithNamespace}' has releases disabled.");
            return null;
        }

        var responseBody = await resp.Content.ReadAsStringAsync();
        if (responseBody is "{\"message\":\"404 Not Found\"}")
            return null;

        return JsonSerializer.Deserialize(responseBody,
            GitLabReleaseJsonResponseSerializerContext.Default.GitLabReleaseJsonResponse);
    }

    public static Task<bool> UploadGenericPackageAsync(
        this BulkUploadGenericPackageCommandArgument arg,
        Project project,
        string filePath)
        => UploadGenericPackageAsync(new UploadGenericPackageCommandArgument(arg, filePath), project);

    public static async Task<bool> UploadGenericPackageAsync(
        this UploadGenericPackageCommandArgument arg,
        Project project)
    {
        HttpResponseMessage response;

        await using (var fileStream = arg.FilePath.OpenRead())
        {
            response = await arg.Http.PutAsync(
                $"api/v4/projects/{project.Id}/packages/generic/{arg.PackageName}/{arg.PackageVersion}/{arg.FilePath.Name}",
                new StreamContent(fileStream)
            );
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            Logger.Error(LogSource.App, "Invalid authorization.");
        if (response.StatusCode == HttpStatusCode.Forbidden)
            Logger.Error(LogSource.App, "Target project has the package registry disabled.");

        return response.IsSuccessStatusCode;
    }

    public static async Task<GetProjectPackagesItem?> FindMatchingPackageAsync(
        this CreateReleaseFromGenericPackageFilesArgument arg,
        Project project)
    {
        var packages = await PaginateAsync(
            arg.Http, 
            $"api/v4/projects/{project.Id}/packages?package_type=generic&sort=desc&order_by=created_at&per_page=100",
            cont => 
                cont.ReadFromJsonAsync(GetProjectPackagesSerializerContext.Default.IEnumerableGetProjectPackagesItem)!,
            _ => Logger.Error(LogSource.App, "Target project has the package registry disabled.")
        );

        return packages?.FirstOrDefault(it => it.Name == arg.PackageName && it.Version == arg.PackageVersion);
    }

    public static Task<IEnumerable<GetPackageFilesItem>?> GetPackageFilesAsync(
        this GetProjectPackagesItem matchingPackage,
        HttpClient http,
        Project project) =>
        PaginateAsync(
            http, 
            $"api/v4/projects/{project.Id}/packages/{matchingPackage.Id}/package_files?per_page=100",
            cont => 
                cont.ReadFromJsonAsync(GetPackageFilesSerializerContext.Default.IEnumerableGetPackageFilesItem)!,
            _ => Logger.Error(LogSource.App, "Target project has the package registry disabled.")
        );


    public static async Task<ReleaseInfo?> CreateReleaseFromGenericPackagesAsync(
        this CreateReleaseFromGenericPackageFilesArgument arg,
        Project project)
    {
        await arg.InitIfNeededAsync(project);

        if (await arg.FindMatchingPackageAsync(project) is not { } matchingPackage)
        {
            Logger.Error(LogSource.App,
                $"Could not create a release because a generic package matching name {arg.PackageName}, version {arg.PackageVersion} on project {arg.Options.ProjectPath} wasn't found.");
            return null;
        }

        if (await matchingPackage.GetPackageFilesAsync(arg.Http, project) is not { } packageFiles)
        {
            Logger.Error(LogSource.App,
                $"Could not create a release because the request to get all package files for package matching name {arg.PackageName}, version {arg.PackageVersion} on project {arg.Options.ProjectPath} failed.");
            return null;
        }

        var gitlabAssetLinks = packageFiles.Select(x => new ReleaseLink
        {
            Name = x.Name,
            LinkType = ReleaseLinkType.Package,
            Url = arg.FormatGitLabUrl(
                $"api/v4/projects/{project.Id}/packages/generic/{arg.PackageName}/{arg.PackageVersion}/{x.Name}")
        }).ToArray();

        try
        {
            return await arg.CreateGitLabClient().GetReleases(project.Id).CreateAsync(new ReleaseCreate
            {
                TagName = arg.PackageVersion,
                Ref = arg.ReleaseRef.EqualsAnyIgnoreCase("null") ? null : arg.ReleaseRef,
                Name = arg.ReleaseTitle ?? arg.PackageVersion,
                Description = arg.ReleaseBody,
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