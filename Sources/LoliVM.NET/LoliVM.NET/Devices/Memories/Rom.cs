using LoliVM.NET.Interfaces;

namespace LoliVM.NET.Devices.Memories;

public sealed class Rom : IMemoryDevice
{
	private readonly byte[] _storage;

	public Rom(int size)
	{
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(size);

		_storage = new byte[size];
	}
	
	public int Size => _storage.Length;

	public bool Load(ReadOnlySpan<byte> firmware)
	{
		if (firmware.Length > _storage.Length)
			return false;

		Array.Clear(_storage);
		firmware.CopyTo(_storage);

		return true;
	}
	
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

	public bool TryWrite(int offset, byte value) => false;

	public void Reset() { }
}