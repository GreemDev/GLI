using GitLabCli.Commands.BulkUploadGenericPackage;
using Gommon;

namespace GitLabCli.Commands.UploadGenericPackage;

public class UploadGenericPackageCommandArgument : CliCommandArgument
{
    public UploadGenericPackageCommandArgument(BulkUploadGenericPackageCommandArgument arg, string filePath) : base(null!)
    {
        PackageName = arg.PackageName;
        PackageVersion = arg.PackageVersion;
        FilePath = new FilePath(filePath, false);
        
        Options = arg.Options;
        AccessToken = Options.AccessToken ?? ReadAccessTokenFromFile();
        InitHttp();
    }
    
    public UploadGenericPackageCommandArgument(Options options) : base(options)
    {
        PackageName = options.InputData.Split('|')[0];
        PackageVersion = options.InputData.Split('|')[1];
        FilePath = new FilePath(options.InputData.Split('|')[2], false);
    }
    
    public string PackageName { get; }
    public string PackageVersion { get; }
    public FilePath FilePath { get; }
}