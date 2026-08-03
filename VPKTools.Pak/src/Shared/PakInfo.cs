namespace VPKTools.Pak.Shared;

public sealed record PakInfo(
    string FilePath,
    string PackageName,
    uint Version,
    bool IsDirVpk,
    int EntryCount,
    long TotalSizeBytes);
