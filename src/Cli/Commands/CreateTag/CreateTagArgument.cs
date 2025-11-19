namespace GitLabCli.Commands.CreateTag;

public class CreateTagArgument : CliCommandArgument
{
    public string TagName { get; }
    public string TagRef { get; }
    public string? Comment { get; }

    public CreateTagArgument(Options options) : base(options)
    {
        TagName = options.InputData.Split('|')[0];
        TagRef = options.InputData.Split('|')[1];

        try
        {
            Comment = options.InputData.Split('|')[2];
        }
        catch
        {
            // ignored
        }
    }
}