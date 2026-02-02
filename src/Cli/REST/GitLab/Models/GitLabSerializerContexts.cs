using System.Text.Json.Serialization;

namespace gli.REST.GitLab;

[JsonSerializable(typeof(IEnumerable<GitLabReleaseJsonResponse>))]
[JsonSerializable(typeof(IEnumerable<GetProjectPackagesItem>))]
[JsonSerializable(typeof(IEnumerable<GetPackageFilesItem>))]
[JsonSerializable(typeof(IEnumerable<MilestoneItem>))]
[JsonSerializable(typeof(IEnumerable<GitLabProject>))]
[JsonSerializable(typeof(IEnumerable<CreateTag>))]
[JsonSerializable(typeof(IEnumerable<CreateRelease>))]
[JsonSerializable(typeof(GitLabReleaseJsonResponse[]))]
[JsonSerializable(typeof(GetProjectPackagesItem[]))]
[JsonSerializable(typeof(GetPackageFilesItem[]))]
[JsonSerializable(typeof(MilestoneItem[]))]
[JsonSerializable(typeof(GitLabProject[]))]
[JsonSerializable(typeof(CreateTag[]))]
[JsonSerializable(typeof(CreateRelease[]))]
public partial class GitLabSerializerContexts : JsonSerializerContext;