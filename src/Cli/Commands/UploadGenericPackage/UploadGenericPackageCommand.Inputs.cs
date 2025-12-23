using System.Net;
using CommandLine;
using gli.CommandLib;
using gli.Helpers;
using Gommon;
using NGitLab.Models;

namespace gli.Commands;

public partial class UploadGenericPackageCommand
{
    public override TimeSpan? HttpRequestTimeout => TimeSpan.FromMinutes(10); //accomodate shitass internet

    public async Task<bool> UploadGenericPackageAsync(
        Project project, FilePath? path = null)
    {
        var toUpload = path ?? FilePath;

        try
        {
            HttpResponseMessage response;

            await using (var fileStream = toUpload.OpenRead())
            {
                response = await Http.PutAsync(
                    $"api/v4/projects/{project.Id}/packages/generic/{PackageName}/{PackageVersion}/{toUpload.Name}",
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
            Logger.Error(LogSource.App, $"Timed out uploading '{toUpload}'.");
            return false;
        }
        catch (Exception e)
        {
            Logger.Error(LogSource.App, $"Errored uploading '{toUpload}'.", e);
            return false;
        }
    }

    internal override Result BeforeExecution()
    {
        if (!Bulk)
        {
            FilePath = new FilePath(FilePathRaw);

            if (FilePath.IsDirectory)
                return Result.ExitCode(ExitCode.FileNotFound,
                    $"Cannot upload a directory. Use the -b/--bulk " +
                    $"flag for that use case; as it lets you finely choose which files to upload with a pattern; " +
                    $"and you can match everything in a folder if you want to as well."
                );
        }

        return base.BeforeExecution();
    }

    [Option('n', "package-name",
        Required = true, HelpText = "The desired name of the generic package.")]
    public string PackageName { get; set; } = null!;

    [Option('v', "package-version",
        Required = true, HelpText = "The desired version of the generic package.")]
    public string PackageVersion { get; set; } = null!;

    [Option('b', "bulk",
        Required = false, Default = false,
        HelpText = "If true, this will upload multiple files matching the pattern provided in the -p/--path argument. Path can be relative.")]
    public bool Bulk { get; set; }

    public FilePath FilePath { get; private set; }

    [Option('p', "path",
        Required = true,
        HelpText =
            "The path of the file to upload. If this is a bulk upload operation, this turns into a file pattern instead of a direct path.")]
    public string FilePathRaw { get; set; } = null!;
}