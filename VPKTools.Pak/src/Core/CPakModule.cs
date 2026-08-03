using VPKTools.Pak.Shared;
using VPKTools.Tier0.Shared.Interfaces;

namespace VPKTools.Pak.Core;

internal sealed class CPakModule : IModule
{
    public void Init(IInterfaceSystem system)
    {
        system.GetInterface<IPakSystem>(PakInterfaceNames.Pak);
        CPakCommands.Register();
    }

    public void Shutdown()
    {
    }
}
