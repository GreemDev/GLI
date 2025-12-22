using gli.CommandLib;
using gli.Helpers;
using Gommon;

namespace gli.Commands;

public class UploadGenericPackageCommand : CliCommand<UploadGenericPackageCommandArgument>
{
    protected override async ValueTask<ExitCode> ExecuteAsync(UploadGenericPackageCommandArgument arg)
    {
        var project = await arg.CreateGitLabClient().Projects.GetByNamespacedPathAsync(arg.ProjectPath);
        if (project is null)
        {
            Logger.Error(LogSource.App, $"Could not find the project '{arg.ProjectPath}' on '{arg.GitLabEndpoint}'.");
            return ExitCode.ProjectNotFound;
        }

        if (arg.Bulk)
        {
            var files = Directory.EnumerateFiles(Environment.CurrentDirectory, arg.FilePathRaw).ToArray();
            if (files.Length is 0)
            {
                Logger.Error(LogSource.App,
                    $"Search pattern '{arg.FilePathRaw}' did not match any files in '{Environment.CurrentDirectory}'");
                return ExitCode.FileNotFound;
            }

            int completedFiles = 0;

            foreach (var filePath in files)
            {
                if (await arg.UploadGenericPackageAsync(project, new FilePath(filePath)))
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
            if (!arg.FilePath.ExistsAsFile)
            {
                Logger.Error(LogSource.App, $"Could not find a file at '{arg.FilePath.FullPath}'.");
                return ExitCode.FileNotFound;
            }

            if (!await arg.UploadGenericPackageAsync(project))
            {
                Logger.Error(LogSource.App, $"'{arg.FilePath.FullPath}' failed to upload.");
                return ExitCode.UploadFailed;
            }

            Logger.Info(LogSource.App,
                $"Uploaded '{arg.FilePath.FullPath}' to the package registry on project {project.NameWithNamespace} (id {project.Id}).");
        }

        return ExitCode.Normal;
    }
}