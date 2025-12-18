using CommandLine;

namespace gli;

public class Options
{
    [Option('l', "write-logs-to-file", Required = false, Default = false,
        HelpText = "Do you want logs written to files?")]
    public bool WriteLogFiles { get; set; }
}