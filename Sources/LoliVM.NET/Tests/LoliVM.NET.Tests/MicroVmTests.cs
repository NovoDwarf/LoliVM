using LoliVM.Tests.Shared;
using LoliVM.NET.Enums;
using Xunit;

namespace LoliVM.NET.Tests;

public sealed class MicroVmTests
{
    [Fact]
    public void DefaultProgram_RunsAndHalts()
    {
        // Arrange
        var vm = TestPrograms.CreateMachine();
        vm.Start();

        // Act
        var executed = vm.Run(10);
        var snapshots = new[] { vm.CaptureSnapshot() };

        // Assert
        Assert.Equal(4, executed);
        Assert.True(vm.Halted);
        Assert.Equal(5, snapshots[^1].Registers.Registers[0]);
        Assert.True(snapshots[^1].Halted);
    }

    [Fact]
    public void Start_AfterFaultAndStopClearsFault()
    {
        // Arrange
        var vm = TestPrograms.CreateMachine();
        vm.LoadProgram([(byte)OpCode.Mov, 9, 1, (byte)OpCode.Halt]);
        vm.Start();
        vm.Run(10);
        var snapshots = new[] { vm.CaptureSnapshot() };
        vm.Stop();

        // Act
        vm.Start();

        // Assert
        Assert.Equal(ExecutionStatus.InvalidRegister, Assert.Single(snapshots).Result!.Value.Status);
        Assert.False(vm.Faulted);
    }

    [Fact]
    public void Run_LimitsInfiniteProgram()
    {
        // Arrange
        var vm = TestPrograms.CreateMachine();
        vm.LoadProgram([(byte)OpCode.Jump, 0, 0]);
        vm.Start();

        // Act
        var executed = vm.Run(20);

        // Assert
        Assert.Equal(20, executed);
        Assert.False(vm.Halted);
        Assert.False(vm.Faulted);
    }

    [Fact]
    public void Reset_ClearsRamAndPreservesFirmwareAndRunningState()
    {
        // Arrange
        var vm = TestPrograms.CreateMachine();
        vm.LoadProgram([
            (byte)OpCode.Load, 1, 0, 2,
            (byte)OpCode.Mov, 0, 42,
            (byte)OpCode.Store, 0, 2, 0,
            (byte)OpCode.Halt
        ]);
        vm.Start();
        vm.Run(4);

        // Act
        vm.Reset();
        vm.Step();
        var snapshots = new[] { vm.CaptureSnapshot() };

        // Assert
        Assert.True(vm.Running);
        var snapshot = Assert.Single(snapshots);
        Assert.Equal(0, snapshot.Registers.Registers[1]);
    }

    [Fact]
    public void Start_WhenAlreadyRunning_DoesNotResetExecution()
    {
        // Arrange
        var vm = TestPrograms.CreateMachine();
        vm.Start();
        vm.Step();

        // Act
        vm.Start();
        vm.Step();
        var snapshots = new[] { vm.CaptureSnapshot() };

        // Assert
        Assert.Single(snapshots);
        Assert.Equal(4, snapshots[0].Registers.Registers[1]);
    }

    [Fact]
    public void LoadProgram_OversizedImageDoesNotReplaceFirmware()
    {
        // Arrange
        var vm = TestPrograms.CreateMachine();
        var program = new byte[257];

        // Act
        var loaded = vm.LoadProgram(program);
        vm.Start();
        var executed = vm.Run(10);

        // Assert
        Assert.False(loaded);
        Assert.Equal(4, executed);
        Assert.True(vm.Halted);
    }

    [Fact]
    public void Run_SubtractsAndJumpsOverInstruction()
    {
        // Arrange
        var program = new byte[]
        {
            (byte)OpCode.Mov, 0, 1,
            (byte)OpCode.Mov, 1, 2,
            (byte)OpCode.Sub, 0, 1,
            (byte)OpCode.Jump, 15, 0,
            (byte)OpCode.Mov, 0, 0,
            (byte)OpCode.Halt
        };
        var vm = TestPrograms.CreateMachine();
        vm.LoadProgram(program);
        vm.Start();

        // Act
        var executed = vm.Run(10);
        var snapshots = new[] { vm.CaptureSnapshot() };

        // Assert
        Assert.Equal(5, executed);
        Assert.Equal(255, snapshots[^1].Registers.Registers[0]);
        Assert.Equal(CpuFlags.Carry | CpuFlags.Negative, snapshots[^1].Status.Flags);
    }
    [Fact]
    public void Stop_PreventsExecution()
    {
        // Arrange
        var vm = TestPrograms.CreateMachine();
        vm.Start();
        vm.Stop();

        // Act
        var executed = vm.Run(5);

        // Assert
        Assert.Equal(0, executed);
    }

    [Fact]
    public void Run_RejectsZeroLimit()
    {
        // Arrange
        var vm = TestPrograms.CreateMachine();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => vm.Run(0));
    }

    [Fact]
    public void LoadProgram_RejectsRunningMachine()
    {
        // Arrange
        var vm = TestPrograms.CreateMachine();
        vm.Start();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => vm.LoadProgram([0]));
    }

    [Fact]
    public void Reset_AfterHaltAllowsProgramToRunAgain()
    {
        // Arrange
        var vm = TestPrograms.CreateMachine();
        vm.Start();
        vm.Run(10);

        // Act
        vm.Reset();
        var snapshots = new[] { vm.CaptureSnapshot() };
        var executed = vm.Run(10);

        // Assert
        Assert.Equal(0, snapshots[0].ProgramCounter.Value);
        Assert.Equal(4, executed);
        Assert.True(vm.Halted);
    }
}
