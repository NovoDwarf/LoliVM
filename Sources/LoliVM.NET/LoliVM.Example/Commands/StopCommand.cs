namespace LoliVM.Example.Commands;

public sealed class StopCommand : ITerminalCommand
{
    public string Name => "stop";
    public string Usage => "stop";

    public bool Execute(TerminalCommandContext context, string[] arguments, CancellationToken cancellationToken)
    {
        context.Session.Stop();
        context.Output.WriteLine("Machine stopped.");
        
        return true;
    }
}
