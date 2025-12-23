using CommandLine;
using gli.CommandLib;
using gli.Helpers;
using Gommon;

namespace gli.Commands;

[Verb("upload-generic-package", aliases: ["ugp"],
    HelpText =
        "Uploads a given file, or many files in bulk that match a pattern, to a project's package registry on a GitLab instance.")]
public partial class UploadGenericPackageCommand : GitLabCommand
{
    protected override async ValueTask<ExitCode> InvokeAsync()
    {
        var project = await CreateGitLabClient().Projects.GetByNamespacedPathAsync(ProjectPath);
        if (project is null)
        {
            Logger.Error(LogSource.App, $"Could not find the project '{ProjectPath}' on '{GitLabEndpoint}'.");
            return ExitCode.ProjectNotFound;
        }

        if (Bulk)
        {
            var files = Directory.EnumerateFiles(Environment.CurrentDirectory, FilePathRaw).ToArray();
            if (files.Length is 0)
            {
                Logger.Error(LogSource.App,
                    $"Search pattern '{FilePathRaw}' did not match any files in '{Environment.CurrentDirectory}'");
                return ExitCode.FileNotFound;
            }

            int completedFiles = 0;

            foreach (var filePath in files)
            {
                if (await UploadGenericPackageAsync(project, new FilePath(filePath)))
                {
                    Logger.Info(LogSource.App,
                        $"'Uploaded {filePath.Replace(Environment.CurrentDirectory, string.Empty)}' to the package registry on project '{project.NameWithNamespace}' (id {project.Id}).");
                    completedFiles++;
                }
            }

            Logger.Info(LogSource.App, $"Finished. {completedFiles}/{files.Length} uploads successful.");
        }
        else
        {
            if (!FilePath.ExistsAsFile)
            {
                Logger.Error(LogSource.App, $"Could not find a file at '{FilePath.FullPath}'.");
                return ExitCode.FileNotFound;
            }

            if (!await UploadGenericPackageAsync(project))
            {
                Logger.Error(LogSource.App, $"'{FilePath.FullPath}' failed to upload.");
                return ExitCode.UploadFailed;
            }

            Logger.Info(LogSource.App,
                $"Uploaded '{FilePath.FullPath}' to the package registry on project {project.NameWithNamespace} (id {project.Id}).");
        }

        return ExitCode.Normal;
    }
}