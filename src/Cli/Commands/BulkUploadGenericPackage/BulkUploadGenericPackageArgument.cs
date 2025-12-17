using System.Net;
using gli.Helpers;
using Gommon;
using NGitLab.Models;

namespace gli.Commands.BulkUploadGenericPackage;

public class BulkUploadGenericPackageCommandArgument : CliCommandArgument
{
    public BulkUploadGenericPackageCommandArgument(Options options)
    {
        PackageName = options.InputData.Split('|')[0];
        PackageVersion = options.InputData.Split('|')[1];
        FilePattern = options.InputData.Split('|')[2];
    }

    public override TimeSpan? HttpRequestTimeout => TimeSpan.FromMinutes(10); //accomodate shitass internet

    public async Task<bool> UploadGenericPackageAsync(Project project, FilePath filePath)
    {
        try
        {
            HttpResponseMessage response;

            await using (var fileStream = filePath.OpenRead())
            {
                response = await Http.PutAsync(
                    $"api/v4/projects/{project.Id}/packages/generic/{PackageName}/{PackageVersion}/{filePath.Name}",
                    new StreamContent(fileStream)
                );
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                Logger.Error(LogSource.App, "Invalid authorization.");

            if (response.StatusCode == HttpStatusCode.Forbidden)
                Logger.Error(LogSource.App, "Target project has the package registry disabled.");

            return response.IsSuccessStatusCode;
        }
        catch (TaskCanceledException)
        {
            Logger.Error(LogSource.App, $"Timed out uploading '{filePath}'; moving onto the next file.");
            return false;
        }
        catch (Exception e)
        {
            Logger.Error(LogSource.App, $"Errored uploading '{filePath}'; moving onto the next file.", e);
            return false;
        }
    }

    public string PackageName { get; }
    public string PackageVersion { get; }
    public string FilePattern { get; }
}