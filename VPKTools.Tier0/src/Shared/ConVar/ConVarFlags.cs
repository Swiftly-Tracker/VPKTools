namespace VPKTools.Tier0.Shared.ConVar;

[Flags]
public enum ConVarFlags
{
    None = 0,
    ReadOnly = 1 << 0,
}
