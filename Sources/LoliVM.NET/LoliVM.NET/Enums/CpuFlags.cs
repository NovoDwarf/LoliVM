namespace LoliVM.NET.Enums;

[Flags]
public enum CpuFlags : byte
{
	None = 0,
	Zero = 1 << 0,
	Carry = 1 << 1,
	Negative = 1 << 2,
	Overflow = 1 << 3
}