namespace GitLabCli.Commands;

public enum ExitCode
{
    UploadFailed = -1,
    Normal = 0,
    FileNotFound = 1,
    ProjectNotFound = 2,
}