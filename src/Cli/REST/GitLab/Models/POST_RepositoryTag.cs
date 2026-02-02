using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace gli.REST.GitLab;

public class CreateTag
{
    [Required]
    [JsonPropertyName("tag_name")]
    public required string Name { get; set; }

    /// <summary>
    /// Can be a commit SHA, another tag name, or branch name.
    /// </summary>
    [Required]
    [JsonPropertyName("ref")]
    public required string Ref { get; set; }

    /// <summary>
    /// (optional) - Puts a comment string under the tag in-UI.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; }
}