using System.Text.Json.Serialization;

namespace GitLabCli.API.GitLab;

[JsonSerializable(typeof(IEnumerable<GitLabReleaseJsonResponse>))]
[JsonSerializable(typeof(IEnumerable<GetProjectPackagesItem>))]
[JsonSerializable(typeof(IEnumerable<GetPackageFilesItem>))]
[JsonSerializable(typeof(IEnumerable<MilestoneItem>))]
[JsonSerializable(typeof(GitLabReleaseJsonResponse[]))]
[JsonSerializable(typeof(GetProjectPackagesItem[]))]
[JsonSerializable(typeof(GetPackageFilesItem[]))]
[JsonSerializable(typeof(MilestoneItem[]))]
public partial class SerializerContexts : JsonSerializerContext;