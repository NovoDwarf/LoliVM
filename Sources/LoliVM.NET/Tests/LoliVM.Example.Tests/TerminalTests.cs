using LoliVM.Example.Tests.Utilities;
using LoliVM.Tests.Shared;
using Xunit;

namespace LoliVM.Example.Tests;

public sealed class TerminalTests
{
    [Fact]
    public async Task Run_ExecutesCommandsAndStopsOnExit()
    {
        // Arrange
        var machine = TestPrograms.CreateMachine();
        var output = new StringWriter();
        using var application = new TerminalTestHost(machine, new StringReader("start\nrun\ndump\nexit\n"), output);

        // Act
        await application.Terminal.RunAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Contains("Executed 4 step(s)", output.ToString());
        Assert.Contains("Halted: True", output.ToString());
        Assert.False(machine.Running);
    }

    [Theory]
    [InlineData("run 0")]
    [InlineData("run -1")]
    [InlineData("run abc")]
    [InlineData("run 1 2")]
    public async Task Run_InvalidLimitPrintsUsage(string command)
    {
        // Arrange
        var output = new StringWriter();
        using var application = new TerminalTestHost(TestPrograms.CreateMachine(), new StringReader(command), output);

        // Act
        await application.Terminal.RunAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Contains("Usage: run [positive instruction limit]", output.ToString());
    }

    [Fact]
    public async Task Run_HandlesUnknownCommandAndEndOfInput()
    {
        // Arrange
        var output = new StringWriter();
        using var application = new TerminalTestHost(TestPrograms.CreateMachine(), new StringReader("unknown\n"), output);

        // Act
        await application.Terminal.RunAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Contains("Unknown command", output.ToString());
    }
    [Fact]
    public async Task Run_ResetClearsTrace()
    {
        // Arrange
        var machine = TestPrograms.CreateMachine();
        var output = new StringWriter();
        using var application = new TerminalTestHost(machine, new StringReader("start\nstep\nreset\ndump\nexit\n"), output);

        // Act
        await application.Terminal.RunAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Contains("Trace: 0 entries, 0 dropped.", output.ToString());
    }
    [Fact]
    public async Task Run_DispatchesStatusAndStopStrategies()
    {
        // Arrange
        var output = new StringWriter();
        using var application = new TerminalTestHost(TestPrograms.CreateMachine(),
            new StringReader("start\nstep\nstop\nstatus\nexit\nstart\n"), output);

        // Act
        await application.Terminal.RunAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Contains("Machine stopped.", output.ToString());
        Assert.Contains("Running: False, Halted: False, Faulted: False", output.ToString());
        Assert.Equal(1, output.ToString().Split("Machine started.").Length - 1);
    }
}
