using LoliVM.NET.Models;

namespace LoliVM.NET.Interfaces;

public interface IInstructionExecutor
{
	public InstructionResult TryExecute(Instruction instruction, CpuContext context);
}