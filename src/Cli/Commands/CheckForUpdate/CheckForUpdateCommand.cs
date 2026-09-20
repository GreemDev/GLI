using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using CommandLine;
using gli.CommandLib;
using gli.Helpers;
using gli.REST.GitHub;
using Gommon;

namespace gli.Commands;

[Verb("check-for-update", aliases: ["update", "upd"], HelpText = "Checks for a new version of GLI from the upstream GitHub repository.")]
public partial class CheckForUpdateCommand : Command
{
    private static readonly Version CurrentVersion = typeof(CheckForUpdateCommand).Assembly.GetName().Version!;
    private static readonly string CurrentVersionString = CurrentVersion.ToString()[..^2];

    protected override async ValueTask<ExitCode> InvokeAsync()
    {
#if DEBUG
        Logger.Info(LogSource.App, "version '16' up to date -kzzkt-");
        return ExitCode.Normal;
#endif

        var latest = await GitHubApi.GetLatestGliReleaseAsync(Http);
        if (latest is null)
            return ExitCode.ObjectNotFound;

        if (!Version.TryParse(latest.Version, out var latestVersion))
        {
            Logger.Error(LogSource.App, $"Retrieved release's tag was not a valid .NET Version format: '{latest.Version}'");
            return ExitCode.OperationFailure;
        }

        if (CurrentVersion >= latestVersion)
        {
            Logger.Info(LogSource.App, $"{CurrentVersionString} is up to date{
                (Download ? "; ignoring download flag" : string.Empty)
            }.");
        }
        else
        {
            Logger.Info(LogSource.App, $"{CurrentVersionString} is out of date, {latestVersion} is now available.");
            Logger.Info(LogSource.App, $"Changes: https://github.com/GreemDev/GLI/compare/{CurrentVersionString}...{latestVersion}");
            if (Download)
            {
                var binaryPath = new FilePath(GetRequiredGliBinaryName(), isDirectory: false);
                var relevantAsset = latest.Assets.FirstOrDefault(x => x.Name == binaryPath);
                if (relevantAsset is null)
                {
                    Logger.Error(LogSource.App,
                        "Could not download; there was no release asset that matched the current platform.");
                }
                else
                {
                    Logger.Info(LogSource.App, "Downloading the latest release...");

                    try
                    {
                        var sw = Stopwatch.StartNew();

                        await using var fs = binaryPath.OpenWrite();

                        await Http.GetAsync(relevantAsset.DownloadUrl)
                            .Then(x => x.Content.ReadAsStreamAsync())
                            .ThenUse(x => x.CopyToAsync(fs));

                        sw.Stop();

                        Logger.Info(LogSource.App,
                            $"Done in {sw.ElapsedMilliseconds}ms. Release was written to '{binaryPath.FullPath}'");
                    }
                    catch (Exception e)
                    {
                        Logger.Error(LogSource.App, "Downloading the release failed.", e);
                        return ExitCode.OperationFailure;
                    }
                }
            }
        }

        return ExitCode.Normal;
    }

    [SuppressMessage("ReSharper", "HeuristicUnreachableCode")]
    public static string GetRequiredGliBinaryName()
    {
        return OperatingSystem.IsWindows()
            ? $"gli-{RuntimeInformation.RuntimeIdentifier}.exe"
            : $"gli-{RuntimeInformation.RuntimeIdentifier}";
    }
}