using GitLabCli.API;
using GitLabCli.Helpers;

namespace GitLabCli.Commands.CreateReleaseFromGenericPackageFiles;

[Command]
public class CreateReleaseFromGenericPackageFilesCommand() : CliCommand<CreateReleaseFromGenericPackageFilesArgument>(CliCommandName.CreateReleaseFromGenericPackageFiles)
{
    protected override CreateReleaseFromGenericPackageFilesArgument CreateArg(Options options) => new(options);

    public override async Task ExecuteAsync(CreateReleaseFromGenericPackageFilesArgument arg)
    {
        var project = await arg.CreateGitLabClient().Projects.GetByNamespacedPathAsync(arg.Options.ProjectPath);
        if (project is null)
        {
            Logger.Error(LogSource.App, $"Could not find the project {arg.Options.ProjectPath} on {arg.Options.GitLabEndpoint}");
            return;
        }
        
        using var http = GitLabRestApi.CreateHttpClient(arg.Options.GitLabEndpoint, arg.AccessToken);

        var releaseInfo = await GitLabRestApi.CreateReleaseFromGenericPackagesAsync(arg, http, project.Id);
        if (releaseInfo != null)
            Logger.Info(LogSource.App, $"Release created at {arg.Options.GitLabEndpoint.TrimEnd('/')}/{arg.Options.ProjectPath}/-/releases/{arg.PackageVersion}");
    }
}