using LoliVM.NET.Enums;

namespace LoliVM.NET.Models;

public readonly record struct InstructionResult(Instruction Instruction, ExecutionStatus Status);