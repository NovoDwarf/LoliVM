using LoliVM.Example.Tests.Utilities;
using LoliVM.Tests.Shared;
using Xunit;

namespace LoliVM.Example.Tests;

public sealed class ProgramEditorTests
{
    [Fact]
    public async Task InteractiveInput_LoadsAndExecutesAssemblyProgram()
    {
        // Arrange
        var machine = TestPrograms.CreateMachine();
        var output = new StringWriter();
        const string input = "edit\nMOV R0, 7\nMOV R1, 3\nADD R0, R1\nSTORE 0x0200, R0\nLOAD R2, 0x0200\nHALT\n.load\nstart\nrun\nexit\n";
        using var application = new TerminalTestHost(machine, new StringReader(input), output);

        // Act
        await application.Terminal.RunAsync(TestContext.Current.CancellationToken);
        var state = machine.CaptureSnapshot();

        // Assert
        Assert.Contains("Loaded 18 bytes.", output.ToString());
        Assert.Contains("Executed 6 step(s).", output.ToString());
        Assert.Equal(10, state.Registers.Registers[2]);
        Assert.True(state.Halted);
    }

    [Fact]
    public async Task AssemblyError_PreservesInputForCorrection()
    {
        // Arrange
        var machine = TestPrograms.CreateMachine();
        var output = new StringWriter();
        const string input = "edit\nMOV R0, 7\nBAD\n.load\n.list\n.undo\nHALT\n.load\nstart\nrun\nexit\n";
        using var application = new TerminalTestHost(machine, new StringReader(input), output);

        // Act
        await application.Terminal.RunAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Contains("Line 2: Unknown instruction", output.ToString());
        Assert.Contains("2: BAD", output.ToString());
        Assert.Contains("Loaded 4 bytes.", output.ToString());
        Assert.Equal(7, machine.CaptureSnapshot().Registers.Registers[0]);
    }

    [Fact]
    public async Task CancelAfterAssemblyError_PreservesOriginalFirmware()
    {
        // Arrange
        var machine = TestPrograms.CreateMachine();
        var output = new StringWriter();
        const string input = "edit\nMOV R0, 99\nBAD\n.load\n.cancel\nstart\nrun\nexit\n";
        using var application = new TerminalTestHost(machine, new StringReader(input), output);

        // Act
        await application.Terminal.RunAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Contains("Program discarded.", output.ToString());
        Assert.Equal(5, machine.CaptureSnapshot().Registers.Registers[0]);
    }

    [Fact]
    public async Task Edit_RejectsRunningMachine()
    {
        // Arrange
        var output = new StringWriter();
        using var application = new TerminalTestHost(TestPrograms.CreateMachine(),
            new StringReader("start\nedit\nstatus\nexit\n"), output);

        // Act
        await application.Terminal.RunAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Contains("Stop the machine before editing", output.ToString());
        Assert.DoesNotContain("asm[", output.ToString());
    }

    [Fact]
    public async Task Clear_DiscardsPreviousSourceBeforeLoading()
    {
        // Arrange
        var machine = TestPrograms.CreateMachine();
        var output = new StringWriter();
        using var application = new TerminalTestHost(machine,
            new StringReader("edit\nBAD\n.clear\nHALT\n.load\nstart\nrun\nexit\n"), output);

        // Act
        await application.Terminal.RunAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Contains("Loaded 1 bytes.", output.ToString());
        Assert.Contains("Executed 1 step(s).", output.ToString());
        Assert.True(machine.Halted);
    }
}
