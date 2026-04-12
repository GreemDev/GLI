using System.Collections.Immutable;
using System.Drawing;
using System.Text;
using CommandLine;
using ForgejoApiClient.Api;
using gli.CommandLib;
using gli.Helpers;
using Gommon;
using JNogueira.Discord.Webhook.Client;

namespace gli.Commands;

[Verb("send-update-message", aliases: ["send-webhook"],
    HelpText = "Sends an embed to a Discord webhook showing information about a Forgejo release. " +
               "The code in this command (namely for finding what files to show) is intended for Ryubing, so your use may vary.")]
public partial class SendUpdateMessageCommand : ForgejoCommand
{
    protected override async ValueTask<ExitCode> InvokeAsync()
    {
        if (await GetReleaseAsync(ProjectOwner, ProjectName) is not { id: not null } release)
        {
            Logger.Error(LogSource.App,
                $"Could not find a release on '{ProjectPath}' with the tag '{ReleaseTag}'.");
            return ExitCode.ObjectNotFound;
        }

        await new DiscordWebhookClient(WebhookUrl)
            .SendToDiscord(
                new DiscordMessage(embeds:
                [
                    CreateEmbed(release)
                ])
            );

        return ExitCode.Normal;
    }

    private DiscordMessageEmbed CreateEmbed(Release release)
        => EmbedThumbnailUrl != null
            ? new(
                title: release.name,
                description: ShowReleaseDescription ? release.body : null,
                color: EmbedColor,
                author: new(release.author?.login_name ?? release.author?.login, iconUrl: release.author?.avatar_url),
                url: release.html_url,
                fields: CreateFields(release.assets),
                thumbnail: new(EmbedThumbnailUrl)
            )
            : new(
                title: release.name,
                description: ShowReleaseDescription ? release.body : null,
                color: EmbedColor,
                author: new(release.author?.login_name ?? release.author?.login, iconUrl: release.author?.avatar_url),
                url: release.html_url,
                fields: CreateFields(release.assets)
            );

    private static DiscordMessageEmbedField[] CreateFields(ICollection<Attachment> assets)
    {
        var windowsX64 = assets.FirstOrDefault(x => x.name.ContainsIgnoreCase("win_x64"));
        var windowsArm64 = assets.FirstOrDefault(x => x.name.ContainsIgnoreCase("win_arm64"));
        var linuxX64 = assets.FirstOrDefault(x => x.name.ContainsIgnoreCase("linux_x64") 
                                                  && !x.name.EndsWithIgnoreCase(".AppImage"));
        var linuxX64AppImage = assets.FirstOrDefault(x => x.name.ContainsIgnoreCase("x64") 
                                                          && x.name.EndsWithIgnoreCase(".AppImage"));
        var macOsUniversal = assets.FirstOrDefault(x => x.name.ContainsIgnoreCase("macos_universal"));
        var macOsArm = assets.FirstOrDefault(x => x.name.ContainsIgnoreCase("macos_arm64"));
        var linuxArm64 = assets.FirstOrDefault(x => x.name.ContainsIgnoreCase("linux_arm64")
                                                    && !x.name.EndsWithIgnoreCase(".AppImage"));
        var linuxArm64AppImage = assets.FirstOrDefault(x => x.name.ContainsIgnoreCase("arm64")
                                                            && x.name.EndsWithIgnoreCase(".AppImage"));
        var androidApk = assets.FirstOrDefault(x => x.name.EndsWithIgnoreCase(".apk"));

        var arrayBuilder = ImmutableArray.CreateBuilder<DiscordMessageEmbedField>();

        applyArtifact(windowsX64, "Windows x64");
        applyArtifact(windowsArm64, "Windows ARM64");
        applyArtifacts((linuxX64, linuxX64AppImage), "Linux x64");
        applyArtifacts((linuxArm64, linuxArm64AppImage), "Linux ARM64");
        applyArtifact(macOsUniversal, "macOS Universal");
        applyArtifact(macOsArm, "macOS (Apple Silicon only)");
        applyArtifact(androidApk, "Android APK");

        return arrayBuilder.ToArray();

        void applyArtifact(Attachment? asset, string friendlyName, bool inline = false)
        {
            if (asset is null)
                return;

            arrayBuilder.Add(new DiscordMessageEmbedField(friendlyName, $"[{asset.name}]({asset.browser_download_url})", inline));
        }

        void applyArtifacts(
            (Attachment? Normal, Attachment? AppImage) asset,
            string friendlyName, bool inline = true)
        {
            var releaseBody = new StringBuilder();

            if (asset.Normal != null)
            {
                releaseBody.AppendLine($"[{asset.Normal.name}]({asset.Normal.browser_download_url})");
            }

            if (asset.AppImage != null)
            {
                releaseBody.AppendLine($"([AppImage]({asset.AppImage.browser_download_url})\u200B)");
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