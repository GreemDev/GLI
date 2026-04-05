using System.Drawing;

namespace gli.Helpers;

/// <summary>
///     Specifies the source of the log message.
/// </summary>
public sealed class LogSource
{
    /// <summary>
    /// Indicates that this log message is from the app itself.
    /// </summary>
    public static readonly LogSource App = new(Color.LawnGreen, "CORE");
    /// <summary>
    ///     Indicates that this log message is from CLI parser.
    /// </summary>
    public static readonly LogSource Cli = new(Color.SteelBlue, "CLI");
    /// <summary>
    /// Indicates that this log message is from the UpdateClient library.
    /// </summary>
    public static readonly LogSource UpdateClient = new(Color.Coral, "UCLIENT");
    /// <summary>
    ///     Indicates that this log message came from an unknown source.
    /// </summary>
    public static readonly LogSource Unknown = new(Color.Fuchsia, "UNKNOWN");

    public Color Color { get; }
    public string Identifier { get; }

    private LogSource(Color color, string id)
    {
        Color = color;
        Identifier = id;
    }
    
    public static bool operator ==(LogSource left, LogSource right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(LogSource left, LogSource right)
    {
        return !left.Equals(right);
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Color, Identifier);
    }

    public static implicit operator Color(LogSource src) => src.Color;
    public static implicit operator string(LogSource src) => src.Identifier;
}

/// <summary>
///     Specifies the severity of the log message.
/// </summary>
public sealed class LogSeverity
{
    /// <summary>
    ///     Logs that contain the most severe level of error. This type of error indicate that immediate attention
    ///     may be required.
    /// </summary>
    public static readonly LogSeverity Critical = new(Color.Maroon, "CRITICAL");
    /// <summary>
    ///     Logs that highlight when the flow of execution is stopped due to a failure.
    /// </summary>
    public static readonly LogSeverity Error = new(Color.DarkRed, "ERROR");
    /// <summary>
    ///     Logs that highlight an abnormal activity in the flow of execution.
    /// </summary>
    public static readonly LogSeverity Warning = new(Color.Yellow, "WARN");
    /// <summary>
    ///     Logs that track the general flow of the application.
    /// </summary>
    public static readonly LogSeverity Info = new(Color.SpringGreen, "INFO");
    /// <summary>
    ///     Logs that are used for interactive investigation during development.
    /// </summary>
    public static readonly LogSeverity Verbose = new(Color.Pink, "VERBOSE");
    /// <summary>
    ///     Logs that contain the most detailed messages.
    /// </summary>
    public static readonly LogSeverity Debug = new(Color.SandyBrown, "DEBUG");

    public Color Color { get; }
    public string Identifier { get; }

    private LogSeverity(Color color, string id)
    {
        Color = color;
        Identifier = id;
    }

    public static bool operator ==(LogSeverity left, LogSeverity right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(LogSeverity left, LogSeverity right)
    {
        return !left.Equals(right);
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Color, Identifier);
    }

    public static implicit operator Color(LogSeverity severity) => severity.Color;
    public static implicit operator string(LogSeverity severity) => severity.Identifier;
}