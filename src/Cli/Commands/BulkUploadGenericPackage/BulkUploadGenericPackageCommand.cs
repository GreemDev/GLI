using gli.CommandLib;
using gli.Helpers;
using Gommon;

namespace gli.Commands.BulkUploadGenericPackage;

public class BulkUploadGenericPackageCommand()
    : CliCommand<BulkUploadGenericPackageCommandArgument>(CliCommandName.BulkUploadGenericPackage)
{
    protected override async ValueTask<ExitCode> ExecuteAsync(BulkUploadGenericPackageCommandArgument arg)
    {
        var files = Directory.EnumerateFiles(Environment.CurrentDirectory, arg.FilePattern).ToArray();
        if (files.Length is 0)
        {
            Logger.Error(LogSource.App,
                $"Search pattern '{arg.FilePattern}' did not match any files in '{Environment.CurrentDirectory}'");
            return ExitCode.FileNotFound;
        }

        var project = await arg.CreateGitLabClient().Projects.GetByNamespacedPathAsync(arg.ProjectPath);
        if (project is null)
        {
            Logger.Error(LogSource.App, $"Could not find the project '{arg.ProjectPath}' on '{arg.GitLabEndpoint}'.");
            return ExitCode.ProjectNotFound;
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

        return ExitCode.Normal;
    }
}