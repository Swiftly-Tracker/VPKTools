using VPKTools.Pak.Shared;
using VPKTools.Pak.Shared.Formatting;
using VPKTools.Tier0.Shared.CommandLine;

internal static class PakCliHandler
{
    public static bool TryRun(ICommandLine cli, IPakSystem pak, out int exitCode)
    {
        exitCode = 0;

        if (!cli.HasParameter("vpk"))
        {
            return false;
        }

        string vpkPath = cli.GetParameterValue("vpk");

        try
        {
            pak.Open(vpkPath);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to open '{vpkPath}': {ex.Message}");
            exitCode = 1;
            return true;
        }

        bool wantsList = cli.HasParameter("list") || cli.HasParameter("output");

        if (wantsList)
        {
            RunList(cli, pak);
            return true;
        }

        if (cli.HasParameter("info"))
        {
            RunInfo(pak);
            return true;
        }

        if (cli.HasParameter("verify"))
        {
            exitCode = RunVerify(pak);
            return true;
        }

        if (cli.HasParameter("find"))
        {
            exitCode = RunFind(cli, pak);
            return true;
        }

        if (cli.HasParameter("extract"))
        {
            exitCode = RunExtract(cli, pak);
            return true;
        }

        if (cli.HasParameter("extractall"))
        {
            exitCode = RunExtractAll(cli, pak);
            return true;
        }

        // -vpk with no action flag: leave the pak open and fall through to the terminal.
        return false;
    }

    private static void RunList(ICommandLine cli, IPakSystem pak)
    {
        string? filter = cli.HasParameter("filter") ? cli.GetParameterValue("filter") : null;
        var entries = pak.GetEntries(filter);
        var lines = entries.Select(PakEntryFormatter.FormatLine).ToList();

        foreach (var line in lines)
        {
            Console.WriteLine(line);
        }

        if (cli.HasParameter("output"))
        {
            string outputPath = cli.GetParameterValue("output");
            File.WriteAllLines(outputPath, lines);
            Console.Error.WriteLine($"Wrote {lines.Count} entries to '{outputPath}'.");
        }
    }

    private static void RunInfo(IPakSystem pak)
    {
        var info = pak.GetInfo();
        Console.WriteLine($"file: {info.FilePath}");
        Console.WriteLine($"name: {info.PackageName}");
        Console.WriteLine($"version: {info.Version}");
        Console.WriteLine($"is_dir_vpk: {info.IsDirVpk}");
        Console.WriteLine($"entries: {info.EntryCount}");
        Console.WriteLine($"total_size: {PrettySize.Format(info.TotalSizeBytes)} ({info.TotalSizeBytes} bytes)");
    }

    private static int RunVerify(IPakSystem pak)
    {
        var result = pak.Verify();
        Console.WriteLine($"hashes: {Describe(result.HashesValid)}");
        Console.WriteLine($"chunk_hashes: {Describe(result.ChunkHashesValid)}");
        Console.WriteLine($"checksums: {Describe(result.ChecksumsValid)}");
        Console.WriteLine($"signature: {Describe(result.SignatureValid)}");

        foreach (var error in result.Errors)
        {
            Console.Error.WriteLine(error);
        }

        bool anyFailed = result.HashesValid == false
            || result.ChunkHashesValid == false
            || result.ChecksumsValid == false
            || result.SignatureValid == false;

        return anyFailed ? 1 : 0;
    }

    private static int RunFind(ICommandLine cli, IPakSystem pak)
    {
        string path = cli.GetParameterValue("find");
        var entry = pak.FindEntry(path);
        if (entry == null)
        {
            Console.Error.WriteLine($"Not found: {path}");
            return 1;
        }

        Console.WriteLine(PakEntryFormatter.FormatLine(entry));
        return 0;
    }

    private static int RunExtract(ICommandLine cli, IPakSystem pak)
    {
        string entryPath = cli.GetParameterValue("extract");
        string destPath = cli.GetParameterValue("dest");

        if (string.IsNullOrEmpty(destPath))
        {
            Console.Error.WriteLine("Missing -dest <path> for -extract.");
            return 1;
        }

        try
        {
            pak.ExtractEntry(entryPath, destPath);
            Console.Error.WriteLine($"Extracted '{entryPath}' to '{destPath}'.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to extract '{entryPath}': {ex.Message}");
            return 1;
        }
    }

    private static int RunExtractAll(ICommandLine cli, IPakSystem pak)
    {
        string destDir = cli.GetParameterValue("extractall");
        string? filter = cli.HasParameter("filter") ? cli.GetParameterValue("filter") : null;

        try
        {
            int count = pak.ExtractAll(destDir, filter);
            Console.Error.WriteLine($"Extracted {count} entries to '{destDir}'.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Extraction failed: {ex.Message}");
            return 1;
        }
    }

    private static string Describe(bool? value) => value switch
    {
        true => "valid",
        false => "invalid",
        null => "not present",
    };
}
