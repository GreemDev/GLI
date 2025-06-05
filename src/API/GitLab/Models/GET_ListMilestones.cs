using System.Text.Json.Serialization;

namespace GitLabCli.API.GitLab;

public class MilestoneItem
{
    [JsonPropertyName("title")] public string Title { get; set; } = null!;
    [JsonPropertyName("description")] public string Description { get; set; } = null!;
    [JsonPropertyName("created_at")] public DateTimeOffset CreatedAt { get; set; }
    [JsonPropertyName("updated_at")] public DateTimeOffset UpdatedAt { get; set; }
    [JsonPropertyName("due_date")] public DateOnly? DueDate { get; set; }
    [JsonPropertyName("start_date")] public DateOnly? StartDate { get; set; }
    [JsonPropertyName("expired")] public bool Expired { get; set; }
    [JsonPropertyName("web_url")] public string WebUrl { get; set; } = null!;
}

[JsonSerializable(typeof(MilestoneItem[]))]
public partial class MilestoneItemSerializerContext : JsonSerializerContext;