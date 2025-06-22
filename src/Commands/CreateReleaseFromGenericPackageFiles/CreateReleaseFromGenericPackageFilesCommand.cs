using GitLabCli.Helpers;

namespace GitLabCli.Commands.CreateReleaseFromGenericPackageFiles;

[Command]
public class CreateReleaseFromGenericPackageFilesCommand() : CliCommand<CreateReleaseFromGenericPackageFilesArgument>(CliCommandName.CreateReleaseFromGenericPackageFiles)
{
    protected override CreateReleaseFromGenericPackageFilesArgument CreateArg(Options options) => new(options);

    public override async Task<ExitCode> ExecuteAsync(CreateReleaseFromGenericPackageFilesArgument arg)
    {
        var project = await arg.CreateGitLabClient().Projects.GetByNamespacedPathAsync(arg.Options.ProjectPath);
        if (project is null)
        {
            Logger.Error(LogSource.App, $"Could not find the project '{arg.Options.ProjectPath}' on '{arg.Options.GitLabEndpoint}'.");
            return ExitCode.ProjectNotFound;
        }

        if (await arg.CreateReleaseFromGenericPackagesAsync(project) is not { } releaseInfo)
            return ExitCode.ObjectNotFound;

        Logger.Info(LogSource.App, $"Release created at '{releaseInfo.Links.Self}'.");
        return ExitCode.Normal;
    }
}