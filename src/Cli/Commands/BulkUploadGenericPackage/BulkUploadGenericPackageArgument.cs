using System.Net;
using CommandLine;
using gli.Helpers;
using Gommon;
using NGitLab.Models;

namespace gli.Commands.BulkUploadGenericPackage;

public class BulkUploadGenericPackageCommandArgument : CliCommandArgument
{
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

    [Option('n', "package-name",
        Required = true, HelpText = "The desired name of the generic package.")]
    public string PackageName { get; set; } = null!;

    [Option('v', "package-version",
        Required = true, HelpText = "The desired version of the generic package.")]
    public string PackageVersion { get; set; } = null!;

    [Option('p', "pattern",
        Required = true, HelpText = "The file pattern to match against the working directory for files to upload.")]
    public string FilePattern { get; set; } = null!;
}