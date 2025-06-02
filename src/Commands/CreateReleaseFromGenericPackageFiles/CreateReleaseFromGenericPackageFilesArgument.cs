namespace GitLabCli.Commands.CreateReleaseFromGenericPackageFiles;

public class CreateReleaseFromGenericPackageFilesArgument : CliCommandArgument
{
    public CreateReleaseFromGenericPackageFilesArgument(Options options) : base(options)
    {
        PackageName = options.InputData.Split('|')[0];
        PackageVersion = options.InputData.Split('|')[1];
        ReleaseRef = options.InputData.Split('|')[2];

        try
        {
            ReleaseTitle = options.InputData.Split('|')[3];
        }
        catch
        {
            ReleaseTitle = null;
        }
        
        try
        {
            ReleaseBody = options.InputData.Split('|')[4];
        }
        catch
        {
            ReleaseBody = null;
        }
    }
    
    public string PackageName { get; }
    public string PackageVersion { get; }
    
    public string ReleaseRef { get; }
    
    public string? ReleaseTitle { get; }
    public string? ReleaseBody { get; }
}