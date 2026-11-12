using LoliVM.NET.Models.Snapshots;

namespace LoliVM.NET.Interfaces;

public interface IMemoryBus
{
	public bool TryRead(ushort address, out byte value);
	
	public bool TryWrite(ushort address, byte value);

	public MemoryBusSnapshot Dump();
}