namespace GitLabCli.Commands.UploadGenericPackage;

public class UploadGenericPackageCommandArgument : CliCommandArgument
{
    public UploadGenericPackageCommandArgument(Options options) : base(options)
    {
        PackageName = options.InputData.Split('|')[0];
        PackageVersion = options.InputData.Split('|')[1];
        FilePattern = options.InputData.Split('|')[2];
    }
    
    public string PackageName { get; }
    public string PackageVersion { get; }
    public string FilePattern { get; }
}