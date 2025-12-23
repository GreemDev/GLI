using System.Collections.Immutable;
using System.Drawing;
using System.Text;
using CommandLine;
using gli.CommandLib;
using gli.REST.GitLab;
using gli.Helpers;
using Gommon;
using JNogueira.Discord.Webhook.Client;

namespace gli.Commands;

[Verb("send-update-message", aliases: ["send-webhook"], 
    HelpText = "Sends an embed to a Discord webhook showing information about a GitLab release. " +
               "The code in this command (namely for finding what files to show) is intended for Ryubing, so your use may vary.")]
public partial class SendUpdateMessageCommand : GitLabCommand
{
    protected override async ValueTask<ExitCode> InvokeAsync()
    {
        var project = await CreateGitLabClient().Projects.GetByNamespacedPathAsync(ProjectPath);
        if (project is null)
        {
            Logger.Error(LogSource.App, $"Could not find the project '{ProjectPath}' on '{GitLabEndpoint}'.");
            return ExitCode.ProjectNotFound;
        }

        if (await GetReleaseAsync(project) is not { } release)
        {
            Logger.Error(LogSource.App,
                $"Could not find a release on '{project.NameWithNamespace}' with the tag '{ReleaseTag}'.");
            return ExitCode.ObjectNotFound;
        }

        var webhookClient = new DiscordWebhookClient(WebhookUrl);

        var message = new DiscordMessage(embeds: [CreateEmbed(release)]);

        await webhookClient.SendToDiscord(message);

        return ExitCode.Normal;
    }

    private DiscordMessageEmbed CreateEmbed(GitLabReleaseJsonResponse release)
        => EmbedThumbnailUrl != null
            ? new(
                title: release.Name,
                description: ShowReleaseDescription ? release.Description : null,
                color: EmbedColor,
                author: new(release.Author.Name, iconUrl: release.Author.AvatarUrl),
                url: release.Links.Self,
                fields: CreateFields(release.Assets),
                thumbnail: new(EmbedThumbnailUrl)
            )
            : new(
                title: release.Name,
                description: ShowReleaseDescription ? release.Description : null,
                color: EmbedColor,
                author: new(release.Author.Name, iconUrl: release.Author.AvatarUrl),
                url: release.Links.Self,
                fields: CreateFields(release.Assets)
            );

    private static DiscordMessageEmbedField[] CreateFields(
        GitLabReleaseJsonResponse.GitLabReleaseAssetsJsonResponse assets)
    {
        var windowsX64 = assets.Links.FirstOrDefault(x => x.AssetName.ContainsIgnoreCase("win_x64"));
        var windowsArm64 = assets.Links.FirstOrDefault(x => x.AssetName.ContainsIgnoreCase("win_arm64"));
        var linuxX64 = assets.Links.FirstOrDefault(x =>
            x.AssetName.ContainsIgnoreCase("linux_x64") && !x.AssetName.EndsWithIgnoreCase(".AppImage"));
        var linuxX64AppImage = assets.Links.FirstOrDefault(x =>
            x.AssetName.ContainsIgnoreCase("x64") && x.AssetName.EndsWithIgnoreCase(".AppImage"));
        var macOsUniversal = assets.Links.FirstOrDefault(x => x.AssetName.ContainsIgnoreCase("macos_universal"));
        var macOsArm = assets.Links.FirstOrDefault(x => x.AssetName.ContainsIgnoreCase("macos_arm64"));
        var linuxArm64 = assets.Links.FirstOrDefault(x =>
            x.AssetName.ContainsIgnoreCase("linux_arm64") && !x.AssetName.EndsWithIgnoreCase(".AppImage"));
        var linuxArm64AppImage = assets.Links.FirstOrDefault(x =>
            x.AssetName.ContainsIgnoreCase("arm64") && x.AssetName.EndsWithIgnoreCase(".AppImage"));
        var androidApk = assets.Links.FirstOrDefault(x => x.AssetName.EndsWithIgnoreCase(".apk"));

        var arrayBuilder = ImmutableArray.CreateBuilder<DiscordMessageEmbedField>();

        applyArtifact(windowsX64, "Windows x64");
        applyArtifact(windowsArm64, "Windows ARM64");
        applyArtifacts((linuxX64, linuxX64AppImage), "Linux x64");
        applyArtifacts((linuxArm64, linuxArm64AppImage), "Linux ARM64");
        applyArtifact(macOsUniversal, "macOS Universal");
        applyArtifact(macOsArm, "macOS (Apple Silicon only)");
        applyArtifact(androidApk, "Android APK");

        return arrayBuilder.ToArray();

        void applyArtifact(GitLabReleaseJsonResponse.AssetLink? asset, string friendlyName, bool inline = false)
        {
            if (asset is null)
                return;

            arrayBuilder.Add(new DiscordMessageEmbedField(friendlyName, $"[{asset.AssetName}]({asset.Url})", inline));
        }

        void applyArtifacts(
            (GitLabReleaseJsonResponse.AssetLink? Normal, GitLabReleaseJsonResponse.AssetLink? AppImage) asset,
            string friendlyName, bool inline = true)
        {
            var releaseBody = new StringBuilder();

            if (asset.Normal != null)
            {
                releaseBody.AppendLine($"[{asset.Normal.AssetName}]({asset.Normal.Url})");
            }

            if (asset.AppImage != null)
            {
                releaseBody.AppendLine($"([AppImage]({asset.AppImage.Url})\u200B)");
            }

            if (releaseBody.Length is 0)
                return;

            arrayBuilder.Add(new DiscordMessageEmbedField(friendlyName, releaseBody.ToString(), inline));
        }
    }
}

public struct DiscordColor
{
    public DiscordColor(int rawValue) => RawValue = rawValue;

    public DiscordColor(Color drawingColor)
    {
        RawValue = drawingColor.R + (drawingColor.G << 8) + (drawingColor.B << 16);
    }

    public int RawValue { get; }

    public static implicit operator DiscordColor(int rawValue)
        => new(rawValue);

    public static implicit operator int(DiscordColor color)
        => color.RawValue;
}