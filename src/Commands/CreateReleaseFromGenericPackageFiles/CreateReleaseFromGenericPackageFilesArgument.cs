using GitLabCli.API.GitLab;
using GitLabCli.Helpers;
using Gommon;
using NGitLab.Models;

namespace GitLabCli.Commands.CreateReleaseFromGenericPackageFiles;

public class CreateReleaseFromGenericPackageFilesArgument : CliCommandArgument
{
    internal bool IsInit { get; private set; }
    
    public CreateReleaseFromGenericPackageFilesArgument(Options options) : base(options)
    {
        PackageName = options.InputData.Split('|')[0];
        PackageVersion = options.InputData.Split('|')[1];
        ReleaseRef = options.InputData.Split('|')[2];

        try
        {
            ReleaseTitle = options.InputData.Split('|')[3];
        }
        catch
        {
            ReleaseTitle = null;
        }
        
        try
        {
            ReleaseBody = options.InputData.Split('|')[4];
        }
        catch
        {
            ReleaseBody = null;
        }
    }

    public async Task InitIfNeededAsync(Project project)
    {
        if (ReleaseBody is null || IsInit) return;
        
        if (ReleaseBody.StartsWithIgnoreCase("rf:"))
            ReleaseBody = await File.ReadAllTextAsync(ReleaseBody[3..]);

        if (ReleaseBody.StartsWithIgnoreCase("msd:"))
        {
            var milestoneTitle = ReleaseBody[4..];

            if (await GitLabRestApi.GetMilestoneByTitleAsync(Http, project, milestoneTitle) is { } milestone)
                ReleaseBody = milestone.Description;
        }

        IsInit = true;
    }
    
    public string PackageName { get; }
    public string PackageVersion { get; }
    
    public string ReleaseRef { get; }
    
    public string? ReleaseTitle { get; }
    public string? ReleaseBody { get; private set; }
}