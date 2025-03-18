namespace GitLabCli.Entities;

public enum LogSource
{
    /// <summary>
    ///     Indicates that this log message is from the app itself.
    /// </summary>
    App = 0,
    /// <summary>
    ///     Indicates that this log message is from CLI parser.
    /// </summary>
    Cli = 1,
    /// <summary>
    ///     Indicates that this log message came from an unknown source.
    /// </summary>
    Unknown = int.MaxValue
}