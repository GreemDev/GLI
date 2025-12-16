using gli.Helpers;

namespace gli.Commands.UploadGenericPackage;

[Command]
public class UploadGenericPackageCommand() : CliCommand<UploadGenericPackageCommandArgument>(CliCommandName.UploadGenericPackage)
{
    protected override async Task<ExitCode> ExecuteAsync(UploadGenericPackageCommandArgument arg)
    {
        if (!arg.FilePath.ExistsAsFile)
        {
            Logger.Error(LogSource.App, $"Could not find a file at '{arg.FilePath.FullPath}'.");
            return ExitCode.FileNotFound;
        }

        var project = await arg.CreateGitLabClient().Projects.GetByNamespacedPathAsync(arg.ProjectPath);
        if (project is null)
        {
            Logger.Error(LogSource.App, $"Could not find the project '{arg.ProjectPath}' on '{arg.GitLabEndpoint}'.");
            return ExitCode.ProjectNotFound;
        }

        if (!await arg.UploadGenericPackageAsync(project))
        {
            Logger.Error(LogSource.App, $"'{arg.FilePath.FullPath}' failed to upload.");
            return ExitCode.UploadFailed;
        }

        Logger.Info(LogSource.App, $"Uploaded '{arg.FilePath.FullPath}' to the package registry on project {project.NameWithNamespace} (id {project.Id}).");
        return ExitCode.Normal;
    }
}