using System.Text;
using gli.Helpers;
using Starscript;

namespace gli.Commands;

public partial class GenerateProfileCommand
{
    private void LoadHeaderFromFile()
    {
        if (FileHeader != null && FileHeaderFile != null)
        {
            Logger.Warn(LogSource.App,
                "-h (--header) and -H (--header-file) should not be used together, as they override the same data.");
            Logger.Warn(LogSource.App,
                "Using content from file (-H/--header-file) if it exists. Using raw content parameter if it doesn't exist.");
        }

        if (FileHeaderFile != null)
        {
            if (File.Exists(FileHeaderFile))
            {
                FileHeader = File.ReadAllText(FileHeaderFile);
                Logger.Info(LogSource.App, $"Successfully read header content from '{Path.GetFullPath(FileHeaderFile)}'.");
            }

            FileHeaderFile = null;
        }
    }

    private bool ApplyFileHeader(StringBuilder sb)
    {
        if (FileHeader != null)
        {
            if (!StarscriptDisabled)
            {
                if (!Parser.TryParse(FileHeader.Replace("\\n", "\n"), out var parserResult))
                {
                    Logger.Error(LogSource.App, "There were errors when parsing the file header script input:");
                    foreach (var pError in parserResult.Errors)
                    {
                        Logger.Error(LogSource.App, $"| {pError}");
                    }

                    sb.AppendLine(FileHeader.Replace("\\n", "\n"));
                }
                else
                {
                    var script = Compiler.SingleCompile(parserResult);

#if DEBUG
                    Logger.Debug(LogSource.App, "Script constants:");
                    foreach (var (idx, constant) in script.Constants.ToArray().Index())
                    {
                        Logger.Debug(LogSource.App, $"{idx}: '{constant}'".ReplaceLineEndings("<newline>"));
                    }

                    Logger.Debug(LogSource.App, "Executing script...");
#endif

                    try
                    {
                        sb.AppendLine(script.Execute(StarscriptHelper.Hypervisor).ToString());
                        Logger.Info(LogSource.App, "Successfully injected a Starscript file header.");
                    }
                    catch (StarscriptException se)
                    {
                        Logger.Error(LogSource.App, se);
                        return false;
                    }
                }
            }
            else
            {
                sb.AppendLine(FileHeader.Replace("\\n", "\n"));
                Logger.Info(LogSource.App, "Successfully injected a string file header.");
            }

            sb.AppendLine();
        }

        return true;
    }
}