using System.Text.Json.Serialization;

namespace gli.REST.GitHub;

[JsonSerializable(typeof(IEnumerable<ReleaseData>))]
[JsonSerializable(typeof(ReleaseData[]))]
public partial class GitHubSerializerContexts : JsonSerializerContext;