namespace VPKTools.Pak.Shared;

public interface IPakSystem
{
    bool IsOpen { get; }
    string? FilePath { get; }

    void Open(string path);
    void Close();

    PakInfo GetInfo();
    IReadOnlyList<PakEntryInfo> GetEntries(string? pathFilter = null);
    PakEntryInfo? FindEntry(string path);

    byte[] ReadEntry(string path);
    void ExtractEntry(string path, string destinationPath);
    int ExtractAll(string destinationDirectory, string? pathFilter = null);

    PakVerifyResult Verify();
}
