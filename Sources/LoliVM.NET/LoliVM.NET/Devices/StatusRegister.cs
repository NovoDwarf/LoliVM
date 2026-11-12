using LoliVM.NET.Enums;
using LoliVM.NET.Models.Snapshots;

namespace LoliVM.NET.Devices;

public sealed class StatusRegister
{
    public CpuFlags Flags { get; private set; } = CpuFlags.None;

    public void Update(CpuFlags flags)
    {
        Flags = flags;
    }

    public void Set(CpuFlags flags)
    {
        Flags |= flags;
    }

    public void Clear(CpuFlags flags)
    {
        Flags &= ~flags;
    }

    public void Reset()
    {
        Flags = CpuFlags.None;
    }

    public bool HasAny(CpuFlags flags)
    {
        return (Flags & flags) != CpuFlags.None;
    }

    public bool HasAll(CpuFlags flags)
    {
        return (Flags & flags) == flags;
    }

    public StatusRegisterSnapshot Dump()
    {
        return new StatusRegisterSnapshot(Flags);
    }
}
