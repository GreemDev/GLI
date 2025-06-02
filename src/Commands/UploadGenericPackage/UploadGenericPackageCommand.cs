using System.Net;
using System.Net.Http.Headers;
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
        
        var http = CreateHttpClient(arg);

        int completedFiles = 0;

        foreach (var filePath in files)
        {
            if (!await UploadGenericPackageAsync(arg, http, project.Id, filePath))
                Logger.Error(LogSource.App, $"{filePath} failed to upload.");
            else completedFiles++;
        }
        
        Logger.Info(LogSource.App, $"Finished. {completedFiles}/{files.Length} uploads successful.");
    }

    private static async Task<bool> UploadGenericPackageAsync(
        UploadGenericPackageCommandArgument arg,
        HttpClient http,
        long projectId,
        string filePath)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Put,
            $"api/v4/projects/{projectId}/packages/generic/{arg.PackageName}/{arg.PackageVersion}/{Path.GetFileName(filePath)}");

        await using var fileStream = File.OpenRead(filePath);

        httpRequest.Content = new StreamContent(fileStream);
        
        var response = await http.SendAsync(httpRequest);
            
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            Logger.Error(LogSource.App, "Invalid authorization.");

        return response.IsSuccessStatusCode;
    }

    private static HttpClient CreateHttpClient(UploadGenericPackageCommandArgument arg)
    {
        return new HttpClient
        {
            BaseAddress = new Uri("https://git.ryujinx.app/"),
            DefaultRequestHeaders =
            {
                UserAgent = { new ProductInfoHeaderValue("GitLabCli", "1.0.0") },
                Authorization = AuthenticationHeaderValue.Parse($"Bearer {arg.AccessToken}")
            }
        };
    }
}