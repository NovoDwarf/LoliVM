using LoliVM.NET.Enums;
using LoliVM.NET.Models;

namespace LoliVM.NET.Interfaces;

public interface IInstructionDecoder
{
	public bool TryDecode(OpCode opcode, out InstructionDescriptor descriptor);
}