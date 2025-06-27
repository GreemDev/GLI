using System.Text.Json.Serialization;
using GitLabCli.API.Helpers;
using NGitLab.Models;

namespace GitLabCli.API.GitLab;

public class GetProjectPackagesItem
{
    [JsonPropertyName("id")] public long Id { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; } = null!;

    [JsonPropertyName("version")] public string Version { get; set; } = null!;

    [JsonPropertyName("package_type")] public string PackageType { get; set; } = null!;

    [JsonPropertyName("created_at")] public DateTimeOffset CreatedAt { get; set; }

    public PaginatedEndpoint<GetPackageFilesItem> GetPackageFiles(IHttpClientProxy http, Project project)
        => PaginatedEndpoint<GetPackageFilesItem>.Builder(http)
            .WithBaseUrl($"api/v4/projects/{project.Id}/packages/{Id}/package_files")
            .WithJsonContentParser(SerializerContexts.Default.IEnumerableGetPackageFilesItem)
            .WithPerPageCount(100)
            .Build();
}