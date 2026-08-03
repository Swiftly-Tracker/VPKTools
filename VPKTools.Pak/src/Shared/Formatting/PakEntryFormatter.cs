using System.Globalization;

namespace VPKTools.Pak.Shared.Formatting;

public static class PakEntryFormatter
{
    public static string FormatLine(PakEntryInfo entry)
        => string.Create(CultureInfo.InvariantCulture,
            $"path={entry.Path} crc={entry.Crc32:X8} size={PrettySize.Format(entry.SizeBytes)} size_in_bytes={entry.SizeBytes}");
}
