using LoliVM.NET.Interfaces;

namespace LoliVM.NET.Devices.Memories;

public sealed class Ram : IMemoryDevice
{
	private readonly byte[] _storage;

	public Ram(int size)
	{
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(size);

		_storage = new byte[size];
	}
	
	public int Size => _storage.Length;
	
	public bool TryRead(int offset, out byte value)
	{
		if ((uint)offset >= (uint)_storage.Length)
		{
			value = 0;
			return false;
		}

		value = _storage[offset];
		return true;
		
	}

	public bool TryWrite(int offset, byte value)
	{
		if ((uint)offset >= (uint)_storage.Length)
			return false;
			
		_storage[offset] = value;
		return true;
		
	}

	public void Reset()
	{
		Array.Clear(_storage);
	}
}