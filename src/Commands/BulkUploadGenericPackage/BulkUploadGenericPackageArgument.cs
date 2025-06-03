namespace GitLabCli.Commands.BulkUploadGenericPackage;

public class BulkUploadGenericPackageCommandArgument : CliCommandArgument
{
    public BulkUploadGenericPackageCommandArgument(Options options) : base(options)
    {
        PackageName = options.InputData.Split('|')[0];
        PackageVersion = options.InputData.Split('|')[1];
        FilePattern = options.InputData.Split('|')[2];
    }
    
    public string PackageName { get; }
    public string PackageVersion { get; }
    public string FilePattern { get; }
}