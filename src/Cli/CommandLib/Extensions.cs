using Gommon;

namespace gli.CommandLib;

public static class Extensions
{
    extension(Result)
    {
        public static Result ExitCode(ExitCode exitCode, string? message) =>
            message is null
                ? Result.Failure(new ExitCodeState(exitCode))
                : Result.Failure(new ExitCodeAndMessageState(exitCode, message));

        public static Result MessageFailure(string message)
            => Result.Failure(new MessageError(Guard.Require(message)));
    }
}