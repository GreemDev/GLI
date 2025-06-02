using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using GitLabCli.Commands.CreateReleaseFromGenericPackageFiles;
using GitLabCli.Commands.UploadGenericPackage;
using GitLabCli.Helpers;
using NGitLab.Models;

namespace GitLabCli.API;

public static class GitLabRestApi
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
    
    public static async Task<bool> UploadGenericPackageAsync(
        UploadGenericPackageCommandArgument arg,
        HttpClient http,
        long projectId,
        string filePath)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Put,
            $"api/v4/projects/{projectId}/packages/generic/{arg.PackageName}/{arg.PackageVersion}/{Path.GetFileName(filePath)}");

        await using var fileStream = File.OpenRead(filePath);

        httpRequest.Content = new StreamContent(fileStream);
        
        var response = await http.SendAsync(httpRequest);
            
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            Logger.Error(LogSource.App, "Invalid authorization.");
        if (response.StatusCode == HttpStatusCode.Forbidden)
            Logger.Error(LogSource.App, "Target project has the package registry disabled.");

        return response.IsSuccessStatusCode;
    }

    public static async Task<GetProjectPackagesItem?> FindMatchingPackageAsync(
        CreateReleaseFromGenericPackageFilesArgument arg,
        HttpClient http,
        long projectId)
    {
        var response = await http.GetAsync($"api/v4/projects/{projectId}/packages");

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            Logger.Error(LogSource.App, "Target project has the package registry disabled.");
            return null;
        }

        var packages =
            await response.Content.ReadFromJsonAsync(GetProjectPackagesSerializerContext.Default
                .IEnumerableGetProjectPackagesItem);

        return packages?.FirstOrDefault(it => it.Name == arg.PackageName && it.Version == arg.PackageVersion);
    }

    public static async Task<IEnumerable<GetPackageFilesItem>?> GetPackageFilesAsync(
        HttpClient http,
        long projectId,
        GetProjectPackagesItem matchingPackage)
    {
        var response = await http.GetAsync($"api/v4/projects/{projectId}/packages/{matchingPackage.Id}/package_files?per_page=100");

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            Logger.Error(LogSource.App, "Target project has the package registry disabled.");
            return null;
        }
        
        return await response.Content.ReadFromJsonAsync(GetPackageFilesSerializerContext.Default.IEnumerableGetPackageFilesItem);
    }
    
    
    public static async Task<ReleaseInfo?> CreateReleaseFromGenericPackagesAsync(
        CreateReleaseFromGenericPackageFilesArgument arg,
        HttpClient http,
        long projectId)
    {
        if (await FindMatchingPackageAsync(arg, http, projectId) is not {} matchingPackage)
        {
            Logger.Error(LogSource.App, $"Could not create a release, because a generic package matching name {arg.PackageName}, version {arg.PackageVersion} on project {arg.Options.ProjectPath} wasn't found.");
            return null;
        }
        
        if (await GetPackageFilesAsync(http, projectId, matchingPackage) is not {} packageFiles)
        {
            Logger.Error(LogSource.App, $"Could not create a release, because a request to get all package files for package matching name {arg.PackageName}, version {arg.PackageVersion} on project {arg.Options.ProjectPath} failed.");
            return null;
        }

        var gitlabAssetLinks = packageFiles.Select(x => new ReleaseLink
        {
            Name = x.Name,
            LinkType = ReleaseLinkType.Package,
            Url =
                $"{arg.Options.GitLabEndpoint.TrimEnd('/')}/api/v4/projects/{projectId}/packages/generic/{arg.PackageName}/{arg.PackageVersion}/{x.Name}"
        }).ToArray();

        try
        {
            return await arg.CreateGitLabClient().GetReleases(projectId).CreateAsync(new ReleaseCreate
            {
                TagName = arg.PackageVersion,
                Ref = arg.ReleaseRef,
                Name = arg.ReleaseTitle ?? arg.PackageVersion,
                Description = arg.ReleaseBody,
                ReleasedAt = matchingPackage.CreatedAt.LocalDateTime,
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