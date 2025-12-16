using gli.Helpers;

namespace gli.Commands.CreateReleaseFromGenericPackageFiles;

[Command]
public class CreateReleaseFromGenericPackageFilesCommand() : CliCommand<CreateReleaseFromGenericPackageFilesArgument>(CliCommandName.CreateReleaseFromGenericPackageFiles)
{
    public override async Task<ExitCode> ExecuteAsync(CreateReleaseFromGenericPackageFilesArgument arg)
    {
        var project = await arg.CreateGitLabClient().Projects.GetByNamespacedPathAsync(arg.ProjectPath);
        if (project is null)
        {
            Logger.Error(LogSource.App, $"Could not find the project '{arg.ProjectPath}' on '{arg.GitLabEndpoint}'.");
            return ExitCode.ProjectNotFound;
        }

        if (await arg.CreateReleaseFromGenericPackagesAsync(project) is not { } releaseInfo)
            return ExitCode.ObjectNotFound;

        Logger.Info(LogSource.App, $"Release created at '{releaseInfo.Links.Self}'.");
        return ExitCode.Normal;
    }
}