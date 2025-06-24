namespace GitLabCli.Commands.CreateTag;

public class CreateTagArgument : CliCommandArgument
{
    public string TagName { get; }
    public string TagRef { get; }

    public CreateTagArgument(Options options) : base(options)
    {
        TagName = options.InputData.Split('|')[0];
        TagRef = options.InputData.Split('|')[1];
    }
}