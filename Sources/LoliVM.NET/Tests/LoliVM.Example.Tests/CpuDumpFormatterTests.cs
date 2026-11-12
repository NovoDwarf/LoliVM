using LoliVM.Example.Utilities;
using LoliVM.NET.Enums;
using LoliVM.Tests.Shared;
using Xunit;

namespace LoliVM.Example.Tests;

public sealed class CpuDumpFormatterTests
{
    [Fact]
    public void Format_ReportsFaultWithoutChangingMachineState()
    {
        // Arrange
        var (cpu, _, _) = TestCpu.Create([0xFE]);
        cpu.Step();
        var snapshot = cpu.CaptureSnapshot();

        // Act
        var text = CpuDumpFormatter.Format(snapshot);

        // Assert
        Assert.Contains("Faulted: True", text);
        Assert.Contains("InvalidInstruction", text);
        Assert.Equal(ExecutionStatus.InvalidInstruction, cpu.LastResult!.Value.Status);
        Assert.Equal(snapshot.ProgramCounter, cpu.CaptureSnapshot().ProgramCounter);
    }
}
