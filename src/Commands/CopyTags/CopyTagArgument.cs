namespace GitLabCli.Commands.CopyTags;

public class CopyTagsArgument : CliCommandArgument
{
    public CopyTagsArgument(Options options) : base(options)
    {
        RepoDir = options.InputData.Split('|')[0];
        try
        {
            _message = options.InputData.Split('|')[1];
        }
        catch (IndexOutOfRangeException)
        {

        }
    }

    private readonly string? _message;
    
    public string RepoDir { get; }

    public string Message => _message ?? "Tag copied from a local repository by GitLabCli";
}