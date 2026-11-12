using LoliVM.NET.Enums;

namespace LoliVM.NET.Devices;

public sealed class InstructionRegister
{
	public OpCode Value { get; private set; }

	public void Load(OpCode opCode)
	{
		Value = opCode;
	}

	public void Reset()
	{
		Value = OpCode.Nop;
	}
}