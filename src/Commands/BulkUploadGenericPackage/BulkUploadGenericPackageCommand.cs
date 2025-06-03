using GitLabCli.API;
using GitLabCli.Helpers;

namespace GitLabCli.Commands.BulkUploadGenericPackage;

[Command]
public class BulkUploadGenericPackageCommand() : CliCommand<BulkUploadGenericPackageCommandArgument>(CliCommandName.BulkUploadGenericPackage)
{
    protected override BulkUploadGenericPackageCommandArgument CreateArg(Options options) => new(options);

    public override async Task<ExitCode> ExecuteAsync(BulkUploadGenericPackageCommandArgument arg)
    {
        var files = Directory.EnumerateFiles(Environment.CurrentDirectory, arg.FilePattern).ToArray();
        if (files.Length is 0)
        {
            Logger.Error(LogSource.App, $"Search pattern '{arg.FilePattern}' did not match any files in '{Environment.CurrentDirectory}'");
            return ExitCode.FileNotFound;
        }

        var project = await arg.CreateGitLabClient().Projects.GetByNamespacedPathAsync(arg.Options.ProjectPath);
        if (project is null)
        {
            Logger.Error(LogSource.App, $"Could not find the project '{arg.Options.ProjectPath}' on '{arg.Options.GitLabEndpoint}'.");
            return ExitCode.ProjectNotFound;
        }

        int completedFiles = 0;

        foreach (var filePath in files)
        {
            if (!await arg.UploadGenericPackageAsync(project.Id, filePath))
                Logger.Error(LogSource.App, $"'{filePath.Replace(Environment.CurrentDirectory, string.Empty)}' failed to upload.");
            else
            {
                Logger.Info(LogSource.App, $"'Uploaded {filePath.Replace(Environment.CurrentDirectory, string.Empty)}' to the package registry on project '{project.NameWithNamespace}' (id {project.Id}).");
                completedFiles++;
            }
        }
        
        Logger.Info(LogSource.App, $"Finished. {completedFiles}/{files.Length} uploads successful.");
        
        arg.Http.Dispose();

        return ExitCode.Normal;
    }
}