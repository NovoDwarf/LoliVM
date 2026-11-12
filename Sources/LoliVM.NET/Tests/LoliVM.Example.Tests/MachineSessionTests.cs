using LoliVM.Example.Models;
using LoliVM.Tests.Shared;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace LoliVM.Example.Tests;

public sealed class MachineSessionTests
{
    [Fact]
    public void ExecuteSteps_CancelledRunDoesNotExecuteInstructions()
    {
        // Arrange
        var machine = TestPrograms.CreateMachine();
        var session = new MachineSession(machine, NullLogger<MachineSession>.Instance);
        session.Start();
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cancellation.Cancel();

        // Act
        var executed = session.ExecuteSteps(100, cancellation.Token);

        // Assert
        Assert.Equal(0, executed);
        Assert.Equal(0, session.Trace.Count);
        Assert.Null(machine.CaptureSnapshot().Result);
    }

    [Fact]
    public void Start_WhenAlreadyRunning_PreservesTraceAndPosition()
    {
        // Arrange
        var session = new MachineSession(TestPrograms.CreateMachine(), NullLogger<MachineSession>.Instance);
        session.Start();
        session.ExecuteSteps(1, TestContext.Current.CancellationToken);

        // Act
        session.Start();
        session.ExecuteSteps(1, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, session.Trace.Count);
        Assert.Equal(6, session.Machine.CaptureSnapshot().ProgramCounter.Value);
    }
}
