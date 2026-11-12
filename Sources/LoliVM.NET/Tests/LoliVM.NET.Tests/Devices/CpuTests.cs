using LoliVM.NET.Devices;
using LoliVM.NET.Interfaces;
using LoliVM.NET.Devices.Buses;
using LoliVM.NET.Devices.Decoders;
using LoliVM.NET.Devices.Memories;
using LoliVM.NET.Models;
using LoliVM.NET.Diagnostics;
using LoliVM.NET.Enums;
using LoliVM.Tests.Shared;
using Xunit;

namespace LoliVM.NET.Tests.Devices;

public sealed class CpuTests
{
    [Fact]
    public void Step_ExecutesMemoryProgramAndRecordsIndependentSnapshots()
    {
        // Arrange
        var (cpu, _, ram) = TestCpu.Create([
            (byte)OpCode.Mov, 0, 42,
            (byte)OpCode.Store, 0, 2, 0,
            (byte)OpCode.Load, 1, 0, 2,
            (byte)OpCode.Halt
        ]);

        var trace = new ExecutionTrace(10);

        // Act
        for (var i = 0; i < 4; i++)
        {
            var result = cpu.Step();
            trace.Record(cpu.CaptureSnapshot());
        }
        var read = ram.TryRead(0, out var value);
        var snapshots = trace.GetSnapshots();
        var afterHalt = cpu.Step();

        // Assert
        Assert.True(cpu.Halted);
        Assert.True(read);
        Assert.Equal(42, value);
        Assert.Equal(4, snapshots.Length);
        Assert.Equal(0, snapshots[0].Registers.Registers[1]);
        Assert.Equal(42, snapshots[^1].Registers.Registers[1]);
        Assert.True(snapshots[^1].Halted);
        Assert.Null(afterHalt);
    }

    [Theory]
    [InlineData(0xFE)]
    [InlineData((byte)OpCode.Mov)]
    public void Step_InvalidOrTruncatedInstructionFaults(byte opcode)
    {
        // Arrange
        var (cpu, _, _) = TestCpu.Create([opcode], romSize: 1);

        // Act
        var result = cpu.Step();

        // Assert
        Assert.True(cpu.Faulted);
        Assert.NotNull(result);
        Assert.NotEqual(ExecutionStatus.Success, result.Value.Status);
    }

    [Fact]
    public void Step_FailedWriteFaultsAndRecordsStatus()
    {
        // Arrange
        var (cpu, _, _) = TestCpu.Create([(byte)OpCode.Store, 0, 0, 0]);

        // Act
        var result = cpu.Step();

        // Assert
        Assert.True(cpu.Faulted);
        var snapshot = cpu.CaptureSnapshot();
        Assert.True(snapshot.Faulted);
        Assert.Equal(ExecutionStatus.MemoryAccessFault, snapshot.Result!.Value.Status);
    }

    [Fact]
    public void Fetch_DecodesLittleEndianAddress()
    {
        // Arrange
        var (cpu, _, _) = TestCpu.Create([(byte)OpCode.Jump, 0x34, 0x12]);

        // Act
        var success = cpu.TryFetchInstruction(out var instruction);

        // Assert
        Assert.True(success);
        Assert.Equal(OpCode.Jump, instruction.OpCode);
        Assert.Equal(0x1234, instruction.Operand0);
    }
    [Fact]
    public void Reset_ClearsFaultAndLastResult()
    {
        // Arrange
        var (cpu, _, _) = TestCpu.Create([(byte)OpCode.Store, 0, 0, 0]);
        cpu.Step();

        // Act
        cpu.Reset();
        var snapshot = cpu.CaptureSnapshot();

        // Assert
        Assert.False(cpu.Faulted);
        Assert.False(cpu.Halted);
        Assert.Equal(0, snapshot.ProgramCounter.Value);
        Assert.Null(snapshot.Result);
    }
    [Fact]
    public void Step_AppliesHaltedResultFromExecutor()
    {
        // Arrange
        var bus = new SimpleMemoryBus();
        bus.Map(0, new Rom(1));
        var cpu = new Cpu(bus, new SimpleInstructionDecoder(), new HaltingExecutor());

        // Act
        var result = cpu.Step();
        var subsequentResult = cpu.Step();

        // Assert
        Assert.Equal(ExecutionStatus.Halted, result!.Value.Status);
        Assert.True(cpu.Halted);
        Assert.Null(subsequentResult);
    }

    private sealed class HaltingExecutor : IInstructionExecutor
    {
        public InstructionResult TryExecute(Instruction instruction, CpuContext context)
        {
            return new InstructionResult(instruction, ExecutionStatus.Halted);
        }
    }
}
