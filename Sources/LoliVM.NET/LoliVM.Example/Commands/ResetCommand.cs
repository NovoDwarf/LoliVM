namespace LoliVM.Example.Commands;

public sealed class ResetCommand : ITerminalCommand
{
    public string Name => "reset";
    public string Usage => "reset";

    public bool Execute(TerminalCommandContext context, string[] arguments, CancellationToken cancellationToken)
    {
        context.Session.Reset();
        context.Output.WriteLine("Machine reset.");
        
        return true;
    }
}
