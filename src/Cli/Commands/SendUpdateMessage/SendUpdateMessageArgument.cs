using System.Drawing;
using CommandLine;
using gli.API.GitLab;
using gli.Helpers;
using Gommon;
using NGitLab.Models;

namespace gli.Commands.SendUpdateMessage;

public class SendUpdateMessageArgument : CliCommandArgument
{
    [Option('t', "release-tag",
        Required = true, HelpText = "The tag the release was made with.")]
    public string ReleaseTag { get; set; }

    [Option('w', "webhook",
        Required = true, HelpText = "Your Discord webhook URL.")]
    public string WebhookUrl { get; set; }

    public DiscordColor EmbedColor { get; private set; }

    [Option('i', "image-url",
        Required = false, Default = null,
        HelpText = "Image URL of the embed's thumbnail image. Must be a direct image URL.")]
    public string? EmbedThumbnailUrl { get; set; }

    [Option('D', "show-release-description",
        Required = false, Default = false,
        HelpText =
            "Pastes the description/body of the release into the embed description. Can cause issues if the release body is massive.")]
    public bool ShowReleaseDescription { get; set; }

    [Option('c', "embed-color",
        Required = true, HelpText = "The name of a color or a raw #RRGGBB hexadecimal number.")]
    public string EmbedColorStr { get; set; }

    internal override Result BeforeExecution()
    {
        if (Enum.TryParse(EmbedColorStr, out KnownColor kc))
        {
            EmbedColor = new DiscordColor(Color.FromKnownColor(kc));
        }
        else
        {
            try
            {
                EmbedColor = Convert.ToInt32(EmbedColorStr.TrimStart('#'), 16);
            }
            catch
            {
                return Result.Failure(new MessageError(
                    "Embed color (second, index 1) item in raw command arguments must be a hexadecimal number representing RGB. No preceding #."));
            }
        }

        return base.BeforeExecution();
    }

    public Task<GitLabReleaseJsonResponse?> GetReleaseAsync(Project project)
        => GitLabRestApi.GetReleaseAsync(Http, project, ReleaseTag);
}