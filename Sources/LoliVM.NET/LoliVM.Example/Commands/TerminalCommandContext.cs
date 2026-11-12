using LoliVM.Example.Models;

namespace LoliVM.Example.Commands;

public sealed class TerminalCommandContext
{
    public TerminalCommandContext(
        MachineSession session,
        TextWriter output,
        IReadOnlyList<ITerminalCommand> commands)
    {
        Session = session;
        Output = output;
        Commands = commands;
    }

    public MachineSession Session { get; }
    public TextWriter Output { get; }
    public IReadOnlyList<ITerminalCommand> Commands { get; }

    public void WriteStatus()
    {
        var machine = Session.Machine;
        Output.WriteLine($"Running: {machine.Running}, Halted: {machine.Halted}, Faulted: {machine.Faulted}");
    }
}
