using LoliVM.NET.Enums;

namespace LoliVM.NET.Models;

public readonly record struct InstructionDescriptor(
	OpCode OpCode,
	OperandKind[] Operands
);