namespace gli.Helpers;

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
    /// Indicates that this log message is from the UpdateClient library.
    /// </summary>
    UpdateClient = 2,
    /// <summary>
    ///     Indicates that this log message came from an unknown source.
    /// </summary>
    Unknown = int.MaxValue
}