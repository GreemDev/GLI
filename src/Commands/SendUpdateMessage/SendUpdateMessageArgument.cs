using GitLabCli.API.GitLab;
using GitLabCli.Helpers;
using NGitLab.Models;

namespace GitLabCli.Commands.SendUpdateMessage;

public class SendUpdateMessageArgument : CliCommandArgument
{
    public SendUpdateMessageArgument(Options options) : base(options)
    {
        ReleaseTag = options.InputData.Split('|')[0];
        
        try
        {
            EmbedColor = Convert.ToInt32(options.InputData.Split('|')[1], 16);
        }
        catch
        {
            throw new ArgumentException(
                "Embed color (second, index 1) item in raw command arguments must be a hexadecimal number representing RGB.");
        }
        
        WebhookUrl = options.InputData.Split('|')[2];
        
        try
        {
            EmbedThumbnailUrl = options.InputData.Split('|')[3];
        }
        catch
        {
            EmbedThumbnailUrl = null;
        }

        try
        {
            ShowReleaseDescription = bool.Parse(options.InputData.Split('|')[4]);
        }
        catch (FormatException e)
        {
            Logger.Error(e);
            ShowReleaseDescription = true;
        }
        catch
        {
            ShowReleaseDescription = true;
        }
    }
    
    public Task<GitLabReleaseJsonResponse?> GetReleaseAsync(Project project) 
        => GitLabRestApi.GetReleaseAsync(Http, project, ReleaseTag);
    
    public string ReleaseTag { get; }
    public DiscordColor EmbedColor { get; }
    public string WebhookUrl { get; }
    public string? EmbedThumbnailUrl { get; }
    
    public bool ShowReleaseDescription { get; }
}