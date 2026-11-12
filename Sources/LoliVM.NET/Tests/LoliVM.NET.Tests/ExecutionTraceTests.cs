using LoliVM.NET.Diagnostics;
using LoliVM.Tests.Shared;
using Xunit;

namespace LoliVM.NET.Tests;

public sealed class ExecutionTraceTests
{
    [Fact]
    public void Record_WhenFull_DropsOldestEntry()
    {
        // Arrange
        var trace = new ExecutionTrace(2);
        var (cpu, _, _) = TestCpu.Create([0, 0, 0]);

        // Act
        for (var i = 0; i < 3; i++)
        {
            var result = cpu.Step();
            trace.Record(cpu.CaptureSnapshot());
        }
        var snapshots = trace.GetSnapshots();

        // Assert
        Assert.Equal(2, trace.Count);
        Assert.Equal(1, trace.DroppedCount);
        Assert.Equal(2, snapshots[0].ProgramCounter.Value);
        Assert.Equal(3, snapshots[1].ProgramCounter.Value);
        Assert.Equal(snapshots, trace.GetSnapshots());
    }

    [Fact]
    public void Clear_RemovesEntriesAndResetsDroppedCount()
    {
        // Arrange
        var trace = new ExecutionTrace(1);
        trace.Record(default);
        trace.Record(default);

        // Act
        trace.Clear();

        // Assert
        Assert.Empty(trace.GetSnapshots());
        Assert.Equal(0, trace.DroppedCount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_RejectsNonPositiveCapacity(int capacity)
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new ExecutionTrace(capacity));
    }
}
