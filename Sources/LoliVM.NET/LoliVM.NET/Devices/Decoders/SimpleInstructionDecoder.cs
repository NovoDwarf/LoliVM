using LoliVM.NET.Enums;
using LoliVM.NET.Interfaces;
using LoliVM.NET.Models;

namespace LoliVM.NET.Devices.Decoders;

public class SimpleInstructionDecoder : IInstructionDecoder
{
	public bool TryDecode(OpCode opcode, out InstructionDescriptor descriptor)
	{
		switch (opcode)
		{
			case OpCode.Mov:
				descriptor = new InstructionDescriptor(opcode, [OperandKind.Register, OperandKind.Immediate8]);
				return true;
			case OpCode.Nop:
				descriptor = new InstructionDescriptor(opcode, [OperandKind.None]);
				return true;
			case OpCode.Add:
			case OpCode.Sub:
				descriptor = new InstructionDescriptor(opcode, [OperandKind.Register, OperandKind.Register]);
				return true;
			case OpCode.Load:
				descriptor = new InstructionDescriptor(opcode, [OperandKind.Register, OperandKind.Address16]);
				return true;
			case OpCode.Store:
				descriptor = new InstructionDescriptor(opcode, [OperandKind.Address16, OperandKind.Register]);
				return true;
			case OpCode.Jump:
				descriptor = new InstructionDescriptor(opcode, [OperandKind.Address16]);
				return true;
			case OpCode.Halt:
				descriptor = new InstructionDescriptor(opcode, [OperandKind.None]);
				return true;
			default:
				descriptor = default;
				return false;
		}
	}
}