using System.Text.Json.Serialization;

namespace gli.REST.GitHub;

public class ReleaseData
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("tag_name")]
    public string Version { get; set; }
    [JsonPropertyName("assets")]
    public List<AssetData> Assets { get; set; }
    
    public class AssetData
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("state")]
        public string State { get; set; }
        [JsonPropertyName("browser_download_url")]
        public string DownloadUrl { get; set; }

    }
}