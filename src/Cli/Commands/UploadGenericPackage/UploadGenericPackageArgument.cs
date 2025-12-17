using System.Net;
using CommandLine;
using gli.Commands.BulkUploadGenericPackage;
using gli.Helpers;
using Gommon;
using NGitLab.Models;

namespace gli.Commands.UploadGenericPackage;

public class UploadGenericPackageCommandArgument : CliCommandArgument
{
    public override TimeSpan? HttpRequestTimeout => TimeSpan.FromMinutes(10); //accomodate shitass internet

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

    internal override Result BeforeExecution()
    {
        FilePath = new FilePath(FilePathRaw);

        if (FilePath.IsDirectory)
            return Result.Failure(new MessageError(
                $"Cannot upload a directory. Use the {nameof(CliCommandName.BulkUploadGenericPackage)} command for that use case; as it lets you finely choose which files to upload with a pattern; and you can match everything in a folder if you want to as well."));

        return base.BeforeExecution();
    }

    [Option('n', "package-name",
        Required = true, HelpText = "The desired name of the generic package.")]
    public string PackageName { get; set; } = null!;

    [Option('v', "package-version",
        Required = true, HelpText = "The desired version of the generic package.")]
    public string PackageVersion { get; set; } = null!;

    public FilePath FilePath { get; private set; }

    [Option('p', "path",
        Required = true, HelpText = "The path of the file to upload.")]
    public string FilePathRaw { get; set; } = null!;
}