using LoliVM.NET.Models.Snapshots;

namespace LoliVM.NET.Devices;

public sealed class ProgramCounter
{
	public ushort Value { get; private set; }

	public void Reset()
	{
		Value = 0;
	}

	public void Advance()
	{
		Value = unchecked((ushort)(Value + 1));
	}

	public void Jump(ushort address)
	{
		Value = address;
	}

	public ProgramCounterSnapshot Dump()
	{
		return new ProgramCounterSnapshot(Value);
	}
}