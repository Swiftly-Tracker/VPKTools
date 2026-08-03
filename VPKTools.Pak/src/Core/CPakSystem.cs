using SteamDatabase.ValvePak;
using VPKTools.Pak.Shared;
using VPKTools.Tier0.Shared.Interfaces;

namespace VPKTools.Pak.Core;

[ExposeInterface(PakInterfaceNames.Pak)]
internal sealed class CPakSystem : IPakSystem
{
    private Package? _package;
    private string? _filePath;

    public bool IsOpen => _package != null;
    public string? FilePath => _filePath;

    public void Open(string path)
    {
        _package?.Dispose();

        var package = new Package();
        package.OptimizeEntriesForBinarySearch(StringComparison.OrdinalIgnoreCase);
        package.Read(path);

        _package = package;
        _filePath = path;
    }

    public void Close()
    {
        _package?.Dispose();
        _package = null;
        _filePath = null;
    }

    public PakInfo GetInfo()
    {
        var package = RequireOpen();
        var entries = AllEntries(package).ToList();

        return new PakInfo(
            _filePath!,
            package.FileName ?? _filePath!,
            package.Version,
            package.IsDirVPK,
            entries.Count,
            entries.Sum(e => (long)e.TotalLength));
    }

    public IReadOnlyList<PakEntryInfo> GetEntries(string? pathFilter = null)
    {
        var package = RequireOpen();
        var entries = AllEntries(package);

        if (!string.IsNullOrEmpty(pathFilter))
        {
            entries = entries.Where(e => e.GetFullPath().Contains(pathFilter, StringComparison.OrdinalIgnoreCase));
        }

        return entries
            .OrderBy(e => e.GetFullPath(), StringComparer.OrdinalIgnoreCase)
            .Select(ToEntryInfo)
            .ToList();
    }

    public PakEntryInfo? FindEntry(string path)
    {
        var package = RequireOpen();
        var entry = package.FindEntry(path);
        return entry == null ? null : ToEntryInfo(entry);
    }

    public byte[] ReadEntry(string path)
    {
        var package = RequireOpen();
        var entry = package.FindEntry(path) ?? throw new FileNotFoundException($"Entry not found in package: {path}", path);
        package.ReadEntry(entry, out var output);
        return output;
    }

    public void ExtractEntry(string path, string destinationPath)
    {
        var data = ReadEntry(path);

        var directory = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllBytes(destinationPath, data);
    }

    public int ExtractAll(string destinationDirectory, string? pathFilter = null)
    {
        var package = RequireOpen();
        var entries = AllEntries(package);

        if (!string.IsNullOrEmpty(pathFilter))
        {
            entries = entries.Where(e => e.GetFullPath().Contains(pathFilter, StringComparison.OrdinalIgnoreCase));
        }

        int count = 0;
        foreach (var entry in entries)
        {
            package.ReadEntry(entry, out var output);
            var destinationPath = Path.Combine(destinationDirectory, entry.GetFullPath());

            var directory = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllBytes(destinationPath, output);
            count++;
        }

        return count;
    }

    public PakVerifyResult Verify()
    {
        var package = RequireOpen();
        var errors = new List<string>();

        bool? hashesValid = TryVerify(package.VerifyHashes, "hashes", errors);
        bool? chunkHashesValid = TryVerify(() => package.VerifyChunkHashes(null), "chunk hashes", errors);
        bool? checksumsValid = TryVerify(() => package.VerifyFileChecksums(null), "file checksums", errors);

        bool? signatureValid;
        try
        {
            signatureValid = package.SignatureSectionSize > 0 ? package.IsSignatureValid() : null;
        }
        catch (Exception ex)
        {
            signatureValid = false;
            errors.Add($"signature: {ex.Message}");
        }

        return new PakVerifyResult(hashesValid, chunkHashesValid, checksumsValid, signatureValid, errors);
    }

    private static bool? TryVerify(Action verify, string label, List<string> errors)
    {
        try
        {
            verify();
            return true;
        }
        catch (Exception ex)
        {
            errors.Add($"{label}: {ex.Message}");
            return false;
        }
    }

    private static IEnumerable<PackageEntry> AllEntries(Package package)
        => package.Entries?.Values.SelectMany(list => list) ?? [];

    private static PakEntryInfo ToEntryInfo(PackageEntry entry)
        => new(entry.GetFullPath(), entry.CRC32, entry.TotalLength);

    private Package RequireOpen()
        => _package ?? throw new InvalidOperationException("No VPK package is currently open.");
}
