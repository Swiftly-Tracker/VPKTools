namespace VPKTools.Pak.Shared;

public sealed record PakVerifyResult(
    bool? HashesValid,
    bool? ChunkHashesValid,
    bool? ChecksumsValid,
    bool? SignatureValid,
    IReadOnlyList<string> Errors);
