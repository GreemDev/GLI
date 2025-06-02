using System.Net;
using System.Net.Http.Headers;
using GitLabCli.API;
using GitLabCli.Helpers;

namespace GitLabCli.Commands.UploadGenericPackage;

[Command]
public class UploadGenericPackageCommand() : CliCommand<UploadGenericPackageCommandArgument>(CliCommandName.UploadGenericPackage)
{
    protected override UploadGenericPackageCommandArgument CreateArg(Options options) => new(options);

    public override async Task ExecuteAsync(UploadGenericPackageCommandArgument arg)
    {
        var files = Directory.EnumerateFiles(Environment.CurrentDirectory, arg.FilePattern).ToArray();
        if (files.Length is 0)
        {
            Logger.Error(LogSource.App, $"Search pattern '{arg.FilePattern}' did not match any files in '{Environment.CurrentDirectory}'");
            return;
        }

        var project = await arg.CreateGitLabClient().Projects.GetByNamespacedPathAsync(arg.Options.ProjectPath);
        if (project is null)
        {
            Logger.Error(LogSource.App, $"Could not find the project {arg.Options.ProjectPath} on {arg.Options.GitLabEndpoint}");
            return;
        }
        
        using var http = GitLabRestApi.CreateHttpClient(arg.Options.GitLabEndpoint, arg.AccessToken);

        int completedFiles = 0;

        foreach (var filePath in files)
        {
            if (!await GitLabRestApi.UploadGenericPackageAsync(arg, http, project.Id, filePath))
                Logger.Error(LogSource.App, $"{filePath.Replace(Environment.CurrentDirectory, string.Empty)} failed to upload.");
            else
            {
                Logger.Info(LogSource.App, $"Uploaded {filePath.Replace(Environment.CurrentDirectory, string.Empty)} to the package registry on project {project.NameWithNamespace} (id {project.Id})");
                completedFiles++;
            }
        }
        
        Logger.Info(LogSource.App, $"Finished. {completedFiles}/{files.Length} uploads successful.");
    }
}