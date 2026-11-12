using LoliVM.NET.Models.Snapshots;

namespace LoliVM.NET.Devices;

public class RegisterFile
{
	private readonly byte[] _registers;

	public RegisterFile(int registers)
	{
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(registers);
		
		_registers = new byte[registers];
	}

	public int Count => _registers.Length;
	public ReadOnlySpan<byte> Registers => _registers;
	
	public bool TryGet(int register, out byte value)
	{
		if ((uint)register >= (uint)_registers.Length)
		{
			value = 0;
			return false;
		}

		value = _registers[register];
		return true;
	}

	public bool TrySet(int register, byte value)
	{
		if ((uint)register >= (uint)_registers.Length)
			return false;
			
		_registers[register] = value;
		return true;
	}

	public void Reset()
	{
		Array.Clear(_registers);
	}
	
	public RegisterFileSnapshot Dump()
	{
		return new RegisterFileSnapshot([.. Registers], Count);
	}
}