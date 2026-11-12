namespace LoliVM.NET.Interfaces;

public interface IMemoryDevice
{
	public int Size { get; }

	public bool TryRead(int offset, out byte value);

	public bool TryWrite(int offset, byte value);

	public void Reset();
}