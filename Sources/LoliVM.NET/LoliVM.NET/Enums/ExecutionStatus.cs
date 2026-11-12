namespace LoliVM.NET.Enums;

public enum ExecutionStatus : byte
{
	Success,
	Halted,
	InvalidInstruction,
	InvalidRegister,
	MemoryAccessFault
}