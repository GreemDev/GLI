using CommandLine;

namespace GitLabCli.Entities.Cli;

public class Options
{
    [Option("instance", Required = false, Default = "https://git.ryujinx.app", HelpText = "Runs the task every time the given time period has elapsed; i.e. 8h45m")]
    public string GitLabEndpoint { get; set; }
    
    [Option("access-token", Required = true, HelpText = "https://git.ryujinx.app/-/user_settings/personal_access_tokens")]
    public string AccessToken { get; set; }
    
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
}