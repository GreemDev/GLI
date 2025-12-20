using CommandLine;
using gli.CommandLib;
using gli.Helpers;
using gli.REST.GitHub;
using Gommon;

namespace gli.Commands;

public class CheckForUpdateArgument : CliCommandArgument
{
    [Option('d', "download", Required = false, Default = false, HelpText = "Download the relevant GLI executable for the current operating system for the latest release into the current working directory.")]
    public bool Download { get; set; }
    
    public IHttpClientProxy Http { get; private set; } = null!;

    public override TimeSpan? HttpRequestTimeout => TimeSpan.FromMinutes(10); //accommodate shitass internet

    internal override Result BeforeExecution()
    {
        Http = GitHubApi.CreateHttpClient(HttpRequestTimeout);
        return Result.Success;
    }
}