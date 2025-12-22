using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using gli.CommandLib;
using gli.Helpers;
using gli.REST.GitHub;
using Gommon;

namespace gli.Commands;

public class CheckForUpdateCommand() : CliCommand<CheckForUpdateArgument>(CliCommandName.CheckForUpdate)
{
    private static readonly Version CurrentVersion = typeof(CheckForUpdateCommand).Assembly.GetName().Version!;
    private static readonly string CurrentVersionString = CurrentVersion.ToString()[..^2];

    protected override async ValueTask<ExitCode> ExecuteAsync(CheckForUpdateArgument arg)
    {
        var latest = await GitHubApi.GetLatestReleaseAsync(arg.Http);
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
                (arg.Download ? "; ignoring download flag" : string.Empty)
            }.");
        }
        else
        {
            Logger.Info(LogSource.App, $"{CurrentVersionString} is out of date, {latestVersion} is now available.");
            Logger.Info(LogSource.App, $"Changes: https://github.com/GreemDev/GLI/compare/{CurrentVersionString}...{latestVersion}");
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

    [SuppressMessage("ReSharper", "HeuristicUnreachableCode")]
    public static string GetRequiredGliBinaryName()
    {
#pragma warning disable CS8519 // The given expression never matches the provided constant.
        if (Program.PlatformExtension is not "%%GLI_PLATFORM_EXTENSION%%")
#pragma warning restore CS8519 // The given expression never matches the provided constant.

        {
            const string result = $"gli-{Program.PlatformExtension}";
            return OperatingSystem.IsWindows() ? $"{result}.exe" : result;
        }

        var archString = RuntimeInformation.OSArchitecture switch
        {
            Architecture.X86 => "x86",
            Architecture.X64 => "x64",
            Architecture.Arm => "arm",
            Architecture.Arm64 => "arm64",
            Architecture.LoongArch64 => "loongarch64",
            _ => throw new ArgumentOutOfRangeException(RuntimeInformation.OSArchitecture.Name)
        };

        if (OperatingSystem.IsWindows())
            return $"gli-win-{archString}.exe";

        string os = OperatingSystem.IsLinux() ? "linux" : "osx"; //windows is handled above because of .exe

        return $"gli-{os}-{archString}";
    }
}