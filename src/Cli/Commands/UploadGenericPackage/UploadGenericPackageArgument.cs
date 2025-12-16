using System.Net;
using gli.Commands.BulkUploadGenericPackage;
using gli.Helpers;
using Gommon;
using NGitLab.Models;

namespace gli.Commands.UploadGenericPackage;

public class UploadGenericPackageCommandArgument : CliCommandArgument
{
    public UploadGenericPackageCommandArgument(Options options)
    {
        PackageName = options.InputData.Split('|')[0];
        PackageVersion = options.InputData.Split('|')[1];
        FilePath = new FilePath(options.InputData.Split('|')[2], false);
    }

    public async Task<bool> UploadGenericPackageAsync(
        Project project)
    {
        try
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
        catch (TaskCanceledException)
        {
            Logger.Error(LogSource.App, $"Timed out uploading '{FilePath}'; moving onto the next file.");
            return false;
        }
        catch (Exception e)
        {
            Logger.Error(LogSource.App, $"Errored uploading '{FilePath}'; moving onto the next file.", e);
            return false;
        }
    }

    public string PackageName { get; }
    public string PackageVersion { get; }
    public FilePath FilePath { get; }
}