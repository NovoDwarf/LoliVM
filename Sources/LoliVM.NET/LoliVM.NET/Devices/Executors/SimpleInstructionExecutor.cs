using LoliVM.NET.Enums;
using LoliVM.NET.Interfaces;
using LoliVM.NET.Models;

namespace LoliVM.NET.Devices.Executors;

public class SimpleInstructionExecutor : IInstructionExecutor
{
    public InstructionResult TryExecute(Instruction instruction, CpuContext context)
    {
        var result = instruction.OpCode switch
        {
            OpCode.Nop => new InstructionResult(instruction, ExecutionStatus.Success),
            OpCode.Mov => ExecuteMov(instruction, context),
            OpCode.Add => ExecuteAdd(instruction, context),
            OpCode.Sub => ExecuteSub(instruction, context),
            OpCode.Load => ExecuteLoad(instruction, context),
            OpCode.Store => ExecuteStore(instruction, context),
            OpCode.Jump => ExecuteJump(instruction, context),
            OpCode.Halt => ExecuteHalt(instruction, context),
            _ => new InstructionResult(instruction, ExecutionStatus.InvalidInstruction)
        };

        return result;
    }

    private InstructionResult ExecuteMov(Instruction instruction, CpuContext context)
    {
        if (instruction.Operand1 > byte.MaxValue)
            return new InstructionResult(instruction, ExecutionStatus.InvalidInstruction);

        if (!context.Registers.TrySet(instruction.Operand0, (byte)instruction.Operand1))
            return new InstructionResult(instruction, ExecutionStatus.InvalidRegister);

        return new InstructionResult(instruction, ExecutionStatus.Success);
    }

    private InstructionResult ExecuteAdd(Instruction instruction, CpuContext context)
    {
        return ExecuteArithmetic(instruction, context, subtract: false);
    }

    private InstructionResult ExecuteSub(Instruction instruction, CpuContext context)
    {
        return ExecuteArithmetic(instruction, context, subtract: true);
    }

    private static InstructionResult ExecuteArithmetic(Instruction instruction, CpuContext context, bool subtract)
    {
        if (!context.Registers.TryGet(instruction.Operand0, out var left)
            || !context.Registers.TryGet(instruction.Operand1, out var right))
        {
            return new InstructionResult(instruction, ExecutionStatus.InvalidRegister);
        }

        var result = subtract ? context.Alu.Sub(left, right) : context.Alu.Add(left, right);
        context.Registers.TrySet(instruction.Operand0, result.Value);
        context.Status.Update(result.Flags);
        return new InstructionResult(instruction, ExecutionStatus.Success);
    }

    private InstructionResult ExecuteLoad(Instruction instruction, CpuContext context)
    {
        if (!context.Registers.TryGet(instruction.Operand0, out _))
            return new InstructionResult(instruction, ExecutionStatus.InvalidRegister);

        if (!context.Bus.TryRead(instruction.Operand1, out var value))
            return new InstructionResult(instruction, ExecutionStatus.MemoryAccessFault);

        if (!context.Registers.TrySet(instruction.Operand0, value))
            return new InstructionResult(instruction, ExecutionStatus.InvalidRegister);

        return new InstructionResult(instruction, ExecutionStatus.Success);
    }

    private InstructionResult ExecuteStore(Instruction instruction, CpuContext context)
    {
        if (!context.Registers.TryGet(instruction.Operand1, out var value))
            return new InstructionResult(instruction, ExecutionStatus.InvalidRegister);

        if (!context.Bus.TryWrite(instruction.Operand0, value))
            return new InstructionResult(instruction, ExecutionStatus.MemoryAccessFault);

        return new InstructionResult(instruction, ExecutionStatus.Success);
    }

    private InstructionResult ExecuteJump(Instruction instruction, CpuContext context)
    {
        context.Pc.Jump(instruction.Operand0);
        return new InstructionResult(instruction, ExecutionStatus.Success);
    }

    private InstructionResult ExecuteHalt(Instruction instruction, CpuContext context)
    {

        return new InstructionResult(instruction, ExecutionStatus.Halted);
    }
}
