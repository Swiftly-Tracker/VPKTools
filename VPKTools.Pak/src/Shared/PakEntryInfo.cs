namespace VPKTools.Pak.Shared;

public sealed record PakEntryInfo(
    string Path,
    uint Crc32,
    long SizeBytes);
