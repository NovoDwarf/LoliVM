using LoliVM.NET.Devices;
using LoliVM.NET.Interfaces;

namespace LoliVM.NET.Models;

public sealed record CpuContext
{
	public required RegisterFile Registers { get; init; }
	public required StatusRegister Status { get; init; }
	public required ProgramCounter Pc { get; init; }
	public required IMemoryBus Bus { get; init; }
	public required Alu Alu { get; init; }

	public bool Halted { get; private set; }
	public bool Faulted { get; private set; }

    internal void Reset()
    {
        Halted = false;
        Faulted = false;
    }

    internal void Halt()
	{
		Halted = true;
	}

	internal void Fault()
	{
		Faulted = true;
	}
}
