using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using gli.CommandLib;
using gli.Helpers;
using gli.REST.GitHub;
using Gommon;

namespace gli.Commands;

public class CheckForUpdateCommand() : CliCommand<CheckForUpdateArgument>(CliCommandName.CheckForUpdate)
{
    protected override async ValueTask<ExitCode> ExecuteAsync(CheckForUpdateArgument arg)
    {
        var latest = await GitHubApi.GetLatestReleaseAsync(arg.Http);
        if (latest is null)
            return ExitCode.ObjectNotFound;

        if (!Version.TryParse(latest.Version, out var latestVersion))
        {
            Logger.Error(LogSource.App, "Retrieved release's tag was not a valid .NET Version format.");
            return ExitCode.OperationFailure;
        }

        var currentVersion = Assembly.GetExecutingAssembly().GetName().Version!;

        if (currentVersion >= latestVersion)
        {
            Logger.Info(LogSource.App, $"{currentVersion.ToString()[..^2]} is up to date.");
            if (arg.Download)
            {
                Logger.Info(LogSource.App, "Nothing new to download; ignoring download flag.");
            }
        }
        else
        {
            Logger.Info(LogSource.App, $"{currentVersion.ToString()[..^2]} is out of date, {latestVersion} is now available.");
            if (arg.Download)
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

                        await arg.Http.GetAsync(relevantAsset.DownloadUrl)
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

    public static string GetRequiredGliBinaryName()
    {
#pragma warning disable CS8520 // The given expression always matches the provided constant.
        if (Program.PlatformExtension is "%%GLI_PLATFORM_EXTENSION%%")
#pragma warning restore CS8520 // The given expression always matches the provided constant.
        {
            var arch = RuntimeInformation.OSArchitecture == Architecture.Arm64 ? "arm64" : "x64";

            if (OperatingSystem.IsWindows())
                return $"gli-win-{arch}.exe";

            string os = OperatingSystem.IsLinux() ? "linux" : "osx"; //windows is handled above because of .exe

            return $"gli-{os}-{arch}";
        }

        // ReSharper disable once HeuristicUnreachableCode
        const string result = $"gli-{Program.PlatformExtension}";
        return OperatingSystem.IsWindows() ? $"{result}.exe" : result;
    }
}