using CommandLine;

namespace GitLabCli;

public class Options
{
    [Option("instance", Required = false, Default = "https://git.ryujinx.app", HelpText = "The target GitLab instance to use.")]
    public string GitLabEndpoint { get; set; }
    
    [Option("access-token", Required = false, Default = null, HelpText = "https://git.ryujinx.app/-/user_settings/personal_access_tokens | If a file next to the executable named '.accesstoken' exists, the contents of that file will be used here. An error will be thrown if that file does not exist and this argument is not provided.")]
    public string? AccessToken { get; set; }
    
    [Option("project", Required = false, HelpText = "The 'owner/project' you are requesting. For example, ryubing/ryujinx.")]
    public string ProjectPath { get; set; }
    
    [Option("command", Required = true, HelpText = "The command defined by the application to invoke.")]
    public CliCommandName Command { get; set; }
    
    [Value(0, MetaName = "input", HelpText = "The raw string passed to the action you are performing.")]
    public string InputData { get; set; }
}

public enum CliCommandName
{
    CreateTag,
    CopyTags,
}