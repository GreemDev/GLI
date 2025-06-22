using System.Text.Json.Serialization;
using GitLabCli.Helpers;
using NGitLab.Models;

namespace GitLabCli.API.GitLab;

public class GetProjectPackagesItem
{
    [JsonPropertyName("id")] public long Id { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; } = null!;

    [JsonPropertyName("version")] public string Version { get; set; } = null!;

    [JsonPropertyName("package_type")] public string PackageType { get; set; } = null!;

    [JsonPropertyName("created_at")] public DateTimeOffset CreatedAt { get; set; }
    
    public Task<IEnumerable<GetPackageFilesItem>?> GetPackageFilesAsync(
        HttpClient http,
        Project project) => 
        http.PaginateAsync($"api/v4/projects/{project.Id}/packages/{Id}/package_files?per_page=100",
            SerializerContexts.Default.IEnumerableGetPackageFilesItem,
            _ => Logger.Error(LogSource.App, "Target project has the package registry disabled.")
        );
}