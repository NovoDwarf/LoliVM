namespace LoliVM.Example.Commands;

public sealed class StatusCommand : ITerminalCommand
{
    public string Name => "status";
    public string Usage => "status";

    public bool Execute(TerminalCommandContext context, string[] arguments, CancellationToken cancellationToken)
    {
        context.WriteStatus();
      
        return true;
    }
}
