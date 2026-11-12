using LoliVM.NET.Enums;

namespace LoliVM.NET.Models;

public readonly record struct Instruction(
	OpCode OpCode,
	ushort Operand0,
	ushort Operand1
);