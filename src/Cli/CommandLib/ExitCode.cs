namespace gli.CommandLib;

public enum ExitCode
{
    NormalSilent = -2,
    UploadFailed = -1,
    Normal = 0,
    FileNotFound = 1,
    ProjectNotFound = 2,
    ObjectNotFound = 3,
    ArgumentParseFailed = 4,
    OperationFailure = 5,
}