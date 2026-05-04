using CommandLine;

namespace gli.Commands;

public partial class GenerateProfileCommand
{
    protected override bool NeedsAuthorization => true;

    [Option('u', "username", Required = true, HelpText = "The user to list repositories for.")]
    public string User { get; set; } = null!;

    [Option('o', "organizations", Default = null, HelpText = "The organizations to list repositories for.",
        Separator = ';')]
    public IEnumerable<string>? Organizations { get; set; } = null!;

    [Option('e', "exclusions", Default = null,
        HelpText = "The fully-qualified names of repositories to ignore; i.e. microsoft/vscode.", Separator = ';')]
    public IEnumerable<string>? Exclusions { get; set; } = null!;

    [Option('i', "case-insensitive-exclusions", Default = false,
        HelpText = "Use a case-insensitive comparer when checking exclusions.")]
    public bool CaseInsensitiveExclusions { get; set; }

    [Option('p', "include-private", Default = false,
        HelpText = "Include private repositories that the token can see.")]
    public bool IncludePrivate { get; set; }

    [Option('h', "header", Default = null,
        HelpText =
            "Header content. Use this for a proper README description of yourself, if desired. Supports Starscript.")]
    public string? FileHeader { get; set; }

    [Option('H', "header-file", Default = null,
        HelpText =
            "Header content, read from a file. Use this for a proper README description of yourself, if desired. Supports Starscript.")]
    public string? FileHeaderFile { get; set; }

    [Option('s', "disable-starscript", Default = false,
        HelpText = "Disable Starscript parsing, compilation, and execution for the header content.")]
    public bool StarscriptDisabled { get; set; } = false;

    [Option("result-file-name", Default = "README",
        HelpText = "The name of the resulting file, not including path or extension.")]
    public string FileResultName { get; set; } = null!;

    [Option("result-location", Default = null,
        HelpText =
            "The folder of the resulting file, not including file name or extension. Defaults to the current working directory.")]
    public string? FileResultPathParent { get; set; } = null!;
}