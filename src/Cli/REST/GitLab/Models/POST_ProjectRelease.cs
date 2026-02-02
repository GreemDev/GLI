using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace gli.REST.GitLab;

public class CreateRelease
{
    /// <summary>
    /// (required) - The tag where the release is created from.
    /// </summary>
    [Required]
    [JsonPropertyName("tag_name")]
    public string TagName { get; set; }

    /// <summary>
    /// (optional) - The description of the release.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// (optional) - The release name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    ///  - Required if tag_name doesn't exist. It can be a commit SHA, a tag name, or a branch name.
    /// </summary>
    [JsonPropertyName("ref")]
    public string? Ref { get; set; }

    /// <summary>
    ///  - The title of each milestone the release is associated with.
    /// </summary>
    [JsonPropertyName("milestones")]
    public string[] Milestones { get; set; }

    /// <summary>
    ///  - Assets containing an array of links.
    /// </summary>
    [JsonPropertyName("assets")]
    public ReleaseAssetsInfo Assets { get; set; }

    /// <summary>
    ///  - The date when the release is/was ready. Defaults to the current time.
    /// </summary>
    [JsonPropertyName("released_at")]
    public DateTime? ReleasedAt { get; set; }
}

public class ReleaseAssetsInfo
{
    [JsonPropertyName("count")]
    public int? Count { get; set; }

    [JsonPropertyName("sources")]
    public ReleaseAssetSource[] Sources { get; set; }

    [JsonPropertyName("links")]
    public ReleaseLink[] Links { get; set; }
}

public class ReleaseAssetSource
{
    [JsonPropertyName("format")]
    public string Format { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }
}

public enum ReleaseLinkType
{
    [EnumMember(Value = "other")]
    Other,
    [EnumMember(Value = "runbook")]
    Runbook,
    [EnumMember(Value = "image")]
    Image,
    [EnumMember(Value = "package")]
    Package,
}

public class ReleaseLink
{
    [JsonPropertyName("id")]
    public long? Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("direct_asset_url")]
    public string DirectAssetUrl { get; set; }
    
    [JsonPropertyName("external")]
    public bool External { get; set; }

    [JsonPropertyName("link_type")]
    public ReleaseLinkType LinkType { get; set; }
}