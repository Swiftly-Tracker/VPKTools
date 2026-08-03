using VPKTools.Pak.Shared;
using VPKTools.Pak.Shared.Formatting;
using VPKTools.Tier0.Shared.Interfaces;
using VPKTools.Tier0.Shared.Terminal;

namespace VPKTools.Pak.Core;

internal static class CPakCommands
{
    public static void Register()
    {
        _ = new ConCommand("pak_open", Open, "Open a VPK file. Usage: pak_open <path>");
        _ = new ConCommand("pak_close", Close, "Close the currently open VPK file.");
        _ = new ConCommand("pak_info", Info, "Show information about the currently open VPK file.");
        _ = new ConCommand("pak_list", List, "List entries in the currently open VPK file. Usage: pak_list [filter]");
        _ = new ConCommand("pak_find", Find, "Find an entry by path. Usage: pak_find <path>");
        _ = new ConCommand("pak_extract", Extract, "Extract a single entry. Usage: pak_extract <entryPath> <destPath>");
        _ = new ConCommand("pak_extractall", ExtractAll, "Extract all entries. Usage: pak_extractall <destDir> [filter]");
        _ = new ConCommand("pak_verify", Verify, "Verify hashes, checksums, and signature of the currently open VPK file.");
        _ = new ConCommand("pak_output", Output, "Write a pretty-format entry listing to a file. Usage: pak_output <filePath> [filter]");
    }

    private static void Open(CommandContext ctx)
    {
        if (ctx.Args.Length < 1)
        {
            ctx.Warn("Usage: pak_open <path>");
            return;
        }

        try
        {
            Pak().Open(ctx.Args[0]);
            ctx.Print($"Opened '{ctx.Args[0]}'.");
        }
        catch (Exception ex)
        {
            ctx.Warn($"Failed to open '{ctx.Args[0]}': {ex.Message}");
        }
    }

    private static void Close(CommandContext ctx)
    {
        Pak().Close();
        ctx.Print("Closed.");
    }

    private static void Info(CommandContext ctx)
    {
        if (!RequireOpen(ctx))
        {
            return;
        }

        var info = Pak().GetInfo();
        ctx.Print($"file: {info.FilePath}");
        ctx.Print($"name: {info.PackageName}");
        ctx.Print($"version: {info.Version}");
        ctx.Print($"is_dir_vpk: {info.IsDirVpk}");
        ctx.Print($"entries: {info.EntryCount}");
        ctx.Print($"total_size: {PrettySize.Format(info.TotalSizeBytes)} ({info.TotalSizeBytes} bytes)");
    }

    private static void List(CommandContext ctx)
    {
        if (!RequireOpen(ctx))
        {
            return;
        }

        string? filter = ctx.Args.Length > 0 ? ctx.Args[0] : null;
        var entries = Pak().GetEntries(filter);

        foreach (var entry in entries)
        {
            ctx.Print(PakEntryFormatter.FormatLine(entry));
        }

        ctx.Print($"{entries.Count} entries.");
    }

    private static void Find(CommandContext ctx)
    {
        if (!RequireOpen(ctx))
        {
            return;
        }

        if (ctx.Args.Length < 1)
        {
            ctx.Warn("Usage: pak_find <path>");
            return;
        }

        var entry = Pak().FindEntry(ctx.Args[0]);
        if (entry == null)
        {
            ctx.Warn($"Not found: {ctx.Args[0]}");
            return;
        }

        ctx.Print(PakEntryFormatter.FormatLine(entry));
    }

    private static void Extract(CommandContext ctx)
    {
        if (!RequireOpen(ctx))
        {
            return;
        }

        if (ctx.Args.Length < 2)
        {
            ctx.Warn("Usage: pak_extract <entryPath> <destPath>");
            return;
        }

        try
        {
            Pak().ExtractEntry(ctx.Args[0], ctx.Args[1]);
            ctx.Print($"Extracted '{ctx.Args[0]}' to '{ctx.Args[1]}'.");
        }
        catch (Exception ex)
        {
            ctx.Warn($"Failed to extract '{ctx.Args[0]}': {ex.Message}");
        }
    }

    private static void ExtractAll(CommandContext ctx)
    {
        if (!RequireOpen(ctx))
        {
            return;
        }

        if (ctx.Args.Length < 1)
        {
            ctx.Warn("Usage: pak_extractall <destDir> [filter]");
            return;
        }

        string? filter = ctx.Args.Length > 1 ? ctx.Args[1] : null;

        try
        {
            int count = Pak().ExtractAll(ctx.Args[0], filter);
            ctx.Print($"Extracted {count} entries to '{ctx.Args[0]}'.");
        }
        catch (Exception ex)
        {
            ctx.Warn($"Extraction failed: {ex.Message}");
        }
    }

    private static void Verify(CommandContext ctx)
    {
        if (!RequireOpen(ctx))
        {
            return;
        }

        var result = Pak().Verify();
        ctx.Print($"hashes: {Describe(result.HashesValid)}");
        ctx.Print($"chunk_hashes: {Describe(result.ChunkHashesValid)}");
        ctx.Print($"checksums: {Describe(result.ChecksumsValid)}");
        ctx.Print($"signature: {Describe(result.SignatureValid)}");

        foreach (var error in result.Errors)
        {
            ctx.Warn(error);
        }
    }

    private static void Output(CommandContext ctx)
    {
        if (!RequireOpen(ctx))
        {
            return;
        }

        if (ctx.Args.Length < 1)
        {
            ctx.Warn("Usage: pak_output <filePath> [filter]");
            return;
        }

        string? filter = ctx.Args.Length > 1 ? ctx.Args[1] : null;
        var entries = Pak().GetEntries(filter);

        try
        {
            File.WriteAllLines(ctx.Args[0], entries.Select(PakEntryFormatter.FormatLine));
            ctx.Print($"Wrote {entries.Count} entries to '{ctx.Args[0]}'.");
        }
        catch (Exception ex)
        {
            ctx.Warn($"Failed to write '{ctx.Args[0]}': {ex.Message}");
        }
    }

    private static bool RequireOpen(CommandContext ctx)
    {
        if (Pak().IsOpen)
        {
            return true;
        }

        ctx.Warn("No VPK file is open. Use pak_open <path> first.");
        return false;
    }

    private static string Describe(bool? value) => value switch
    {
        true => "valid",
        false => "invalid",
        null => "not present",
    };

    private static IPakSystem Pak()
        => InterfaceSystem.GetInterface<IPakSystem>(PakInterfaceNames.Pak)!;
}
