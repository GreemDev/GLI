using System.Net;
using GitLabCli.Commands.BulkUploadGenericPackage;
using GitLabCli.Helpers;
using Gommon;
using NGitLab.Models;

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
        InitHttp(TimeSpan.FromMinutes(5));
    }
    
    public UploadGenericPackageCommandArgument(Options options) : base(options)
    {
        PackageName = options.InputData.Split('|')[0];
        PackageVersion = options.InputData.Split('|')[1];
        FilePath = new FilePath(options.InputData.Split('|')[2], false);
    }
    
    public async Task<bool> UploadGenericPackageAsync(
        Project project)
    {
        HttpResponseMessage response;
        
        await using (var fileStream = FilePath.OpenRead())
        {
            response = await Http.PutAsync(
                $"api/v4/projects/{project.Id}/packages/generic/{PackageName}/{PackageVersion}/{FilePath.Name}",
                new StreamContent(fileStream)
            );
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            Logger.Error(LogSource.App, "Invalid authorization.");
        if (response.StatusCode == HttpStatusCode.Forbidden)
            Logger.Error(LogSource.App, "Target project has the package registry disabled.");

        return response.IsSuccessStatusCode;
    }
    
    public string PackageName { get; }
    public string PackageVersion { get; }
    public FilePath FilePath { get; }
}