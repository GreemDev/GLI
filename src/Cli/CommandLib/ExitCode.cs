using Gommon;

namespace gli.CommandLib;

public struct ExitCodeState : IErrorState
{
    public readonly ExitCode Code;

    public ExitCodeState(ExitCode exitCode)
    {
        Code = exitCode;
    }

    public static implicit operator ExitCodeState(ExitCode exitCode) => new(exitCode);
}

public struct ExitCodeAndMessageState : IErrorState
{
    public readonly ExitCode Code;
    public readonly string Message;

    public ExitCodeAndMessageState(ExitCode exitCode, string message)
    {
        Code = exitCode;
        Message = message;
    }
}

public enum ExitCode : byte
{
    Normal = 0,
    FileNotFound = 1,
    ProjectNotFound = 2,
    ObjectNotFound = 3,
    ArgumentParseFailed = 4,
    OperationFailure = 5,
    UploadFailed = 6,
    NormalSilent = byte.MaxValue,
}