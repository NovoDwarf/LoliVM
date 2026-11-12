using System.Collections.Immutable;
using LoliVM.NET.Enums;

namespace LoliVM.NET.Models.Snapshots;

public readonly record struct CpuSnapshot(
    InstructionResult? Result,
    RegisterFileSnapshot Registers,
    StatusRegisterSnapshot Status,
    ProgramCounterSnapshot ProgramCounter,
    MemoryBusSnapshot MemoryBus
    )
{
    public bool Halted { get; init; }
    public bool Faulted { get; init; }


}

public readonly record struct RegisterFileSnapshot(byte[] Registers, int Count);

public readonly record struct MemoryMappingSnapshot(int Start, int End, MemoryDeviceSnapshot Device);

public readonly record struct MemoryDeviceSnapshot(string Name);

public readonly record struct StatusRegisterSnapshot(CpuFlags Flags);

public readonly record struct ProgramCounterSnapshot(ushort Value);

public readonly record struct MemoryBusSnapshot(ImmutableArray<MemoryMappingSnapshot> Mappings);
