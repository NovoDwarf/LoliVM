using LoliVM.NET.Devices;
using LoliVM.NET.Devices.Buses;
using LoliVM.NET.Devices.Executors;
using LoliVM.NET.Devices.Memories;
using LoliVM.NET.Enums;
using LoliVM.NET.Models;
using Xunit;

namespace LoliVM.NET.Tests.Devices.Executors;

public sealed class SimpleInstructionExecutorTests
{
    private readonly SimpleInstructionExecutor _executor = new();
    private readonly CpuContext _context;

    public SimpleInstructionExecutorTests()
    {
        var bus = new SimpleMemoryBus();
        bus.Map(0x200, new Ram(1));
        _context = new CpuContext
        {
            Registers = new RegisterFile(4),
            Status = new StatusRegister(),
            Pc = new ProgramCounter(),
            Bus = bus,
            Alu = new Alu()
        };
    }

    [Theory]
    [InlineData(OpCode.Mov, 4, 1, ExecutionStatus.InvalidRegister)]
    [InlineData(OpCode.Mov, 0, 256, ExecutionStatus.InvalidInstruction)]
    [InlineData(OpCode.Add, 0, 4, ExecutionStatus.InvalidRegister)]
    [InlineData(OpCode.Sub, 4, 0, ExecutionStatus.InvalidRegister)]
    [InlineData(OpCode.Load, 4, 512, ExecutionStatus.InvalidRegister)]
    [InlineData(OpCode.Load, 0, 0, ExecutionStatus.MemoryAccessFault)]
    [InlineData(OpCode.Store, 512, 4, ExecutionStatus.InvalidRegister)]
    [InlineData(OpCode.Store, 0, 0, ExecutionStatus.MemoryAccessFault)]
    [InlineData((OpCode)0xFE, 0, 0, ExecutionStatus.InvalidInstruction)]
    public void Execute_InvalidOperands_ReturnsFailure(OpCode opcode, ushort operand0, ushort operand1, ExecutionStatus expected)
    {
        // Arrange
        var instruction = new Instruction(opcode, operand0, operand1);

        // Act
        var result = _executor.TryExecute(instruction, _context);

        // Assert
        Assert.Equal(expected, result.Status);
    }

    [Fact]
    public void Execute_ArithmeticUpdatesDestinationAndFlags()
    {
        // Arrange
        _context.Registers.TrySet(0, 0);
        _context.Registers.TrySet(1, 1);
        var instruction = new Instruction(OpCode.Sub, 0, 1);

        // Act
        var result = _executor.TryExecute(instruction, _context);
        var read = _context.Registers.TryGet(0, out var value);

        // Assert
        Assert.Equal(ExecutionStatus.Success, result.Status);
        Assert.True(read);
        Assert.Equal(255, value);
        Assert.Equal(CpuFlags.Carry | CpuFlags.Negative, _context.Status.Flags);
    }

    [Fact]
    public void Execute_JumpAndHaltUpdateContext()
    {
        // Arrange
        var jump = new Instruction(OpCode.Jump, 0x1234, 0);
        var halt = new Instruction(OpCode.Halt, 0, 0);

        // Act
        var jumpResult = _executor.TryExecute(jump, _context);
        var haltResult = _executor.TryExecute(halt, _context);

        // Assert
        Assert.Equal(ExecutionStatus.Success, jumpResult.Status);
        Assert.Equal(0x1234, _context.Pc.Value);
        Assert.Equal(ExecutionStatus.Halted, haltResult.Status);
        Assert.False(_context.Halted);
    }
}
