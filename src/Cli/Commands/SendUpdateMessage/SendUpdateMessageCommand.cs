using System.Collections.Immutable;
using System.Text;
using GitLabCli.API.GitLab;
using GitLabCli.Helpers;
using Gommon;
using JNogueira.Discord.Webhook.Client;

namespace GitLabCli.Commands.SendUpdateMessage;

[Command]
public class SendUpdateMessageCommand() : CliCommand<SendUpdateMessageArgument>(CliCommandName.SendUpdateMessage)
{
    public override async Task<ExitCode> ExecuteAsync(SendUpdateMessageArgument arg)
    {
        var project = await arg.CreateGitLabClient().Projects.GetByNamespacedPathAsync(arg.Options.ProjectPath);
        if (project is null)
        {
            Logger.Error(LogSource.App, $"Could not find the project '{arg.Options.ProjectPath}' on '{arg.Options.GitLabEndpoint}'.");
            return ExitCode.ProjectNotFound;
        }

        if (await arg.GetReleaseAsync(project) is not { } release)
        {
            Logger.Error(LogSource.App, $"Could not find a release on '{project.NameWithNamespace}' with the tag '{arg.ReleaseTag}'.");
            return ExitCode.ObjectNotFound;
        }

        var webhookClient = new DiscordWebhookClient(arg.WebhookUrl);

        var message = new DiscordMessage(embeds: [CreateEmbed(arg, release)]);

        await webhookClient.SendToDiscord(message);

        return ExitCode.Normal;
    }

    private static DiscordMessageEmbed CreateEmbed(SendUpdateMessageArgument arg, GitLabReleaseJsonResponse release)
        => arg.EmbedThumbnailUrl != null
            ? new(
                title: release.Name,
                description: arg.ShowReleaseDescription ? release.Description : null,
                color: arg.EmbedColor,
                author: new(release.Author.Name, iconUrl: release.Author.AvatarUrl),
                url: release.Links.Self,
                fields: CreateFields(release.Assets),
                thumbnail: new(arg.EmbedThumbnailUrl)
            )
            : new(
                title: release.Name,
                description: arg.ShowReleaseDescription ? release.Description : null,
                color: arg.EmbedColor,
                author: new(release.Author.Name, iconUrl: release.Author.AvatarUrl),
                url: release.Links.Self,
                fields: CreateFields(release.Assets)
            );

    private static DiscordMessageEmbedField[] CreateFields(GitLabReleaseJsonResponse.GitLabReleaseAssetsJsonResponse assets)
    {
        var windowsX64 = assets.Links.FirstOrDefault(x => x.AssetName.ContainsIgnoreCase("win_x64"));
        var windowsArm64 = assets.Links.FirstOrDefault(x => x.AssetName.ContainsIgnoreCase("win_arm64"));
        var linuxX64 = assets.Links.FirstOrDefault(x => x.AssetName.ContainsIgnoreCase("linux_x64") && !x.AssetName.EndsWithIgnoreCase(".AppImage"));
        var linuxX64AppImage = assets.Links.FirstOrDefault(x => x.AssetName.ContainsIgnoreCase("x64") && x.AssetName.EndsWithIgnoreCase(".AppImage"));
        var macOsUniversal = assets.Links.FirstOrDefault(x => x.AssetName.ContainsIgnoreCase("macos_universal"));
        var macOsArm = assets.Links.FirstOrDefault(x => x.AssetName.ContainsIgnoreCase("macos_arm64"));
        var linuxArm64 = assets.Links.FirstOrDefault(x => x.AssetName.ContainsIgnoreCase("linux_arm64") && !x.AssetName.EndsWithIgnoreCase(".AppImage"));
        var linuxArm64AppImage = assets.Links.FirstOrDefault(x => x.AssetName.ContainsIgnoreCase("arm64") && x.AssetName.EndsWithIgnoreCase(".AppImage"));
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

    public int RawValue { get; }

    public static implicit operator DiscordColor(int rawValue)
        => new(rawValue);

    public static implicit operator int(DiscordColor color)
        => color.RawValue;
}