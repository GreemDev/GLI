using CommandLine;
using gli.Helpers;
using Gommon;
using Ryujinx.Systems.Update.Client;

namespace gli.CommandLib;

public abstract class UpdateServerCliCommandArgument : CliCommandArgument
{
    protected abstract bool NeedsAuthorization { get; }

    [Option('R', "raw", Required = false,
        HelpText = "Causes the logger to output directly to stdout instead of using the custom logger.")]
    public bool LogRaw { get; set; }

    [Option('S', "server-url", Required = false, Default = "https://update.ryujinx.app",
        HelpText =
            "The publicly accessible URL of your Ryubing UpdateServer instance.")]
    public string UpdateServerEndpoint { get; set; } = null!;

    [Option('t', "access-token", Required = false, Default = null,
        HelpText =
            "Your custom admin token in your Ryubing UpdateServer instance. If a file next to the executable named '.admintoken' exists, the contents of that file will be used here. An error will be thrown if that file does not exist and this argument is not provided.")]
    public string? AdminToken { get; set; }

    protected string ReadAdminTokenFromFile()
    {
        var fp = new FilePath(Environment.CurrentDirectory) / ".admintoken";
        if (!fp.ExistsAsFile)
        {
            if (!NeedsAuthorization)
                return null!;

            throw new FileNotFoundException(
                "Could not find an .admintoken file. Either provide the argument '--admin-token' or create the file.");
        }

        return fp.ReadAllText();
    }

    public UpdateClient UpdateClient { get; private set; } = null!;

    /// <remarks>ALWAYS call the base implementation when overriding! Respect the returned result so long as the derived type needs to authenticate with the Update Server.</remarks>
    internal override Result BeforeExecution()
    {
        _ = base.BeforeExecution();

        try
        {
            AdminToken ??= ReadAdminTokenFromFile();
        }
        catch (FileNotFoundException fnfe)
        {
            return Result.Failure(new MessageError(fnfe.Message));
        }

        UpdateClient = UpdateClient.Builder()
            .WithServerEndpoint(UpdateServerEndpoint)
            .WithAccessToken(AdminToken)
            .WithLogger((format, args, caller) =>
            {
                string message = args.Length is 0 ? format : format.Format(args);
                if (LogRaw)
                {
                    Console.WriteLine($"UpdateClient: {caller} -> {message}");
                }
                else
                {
                    Logger.Info(LogSource.UpdateClient,
                        message,
                        new InvocationInfo(caller));
                }
            });

        return Result.Success;
    }
}