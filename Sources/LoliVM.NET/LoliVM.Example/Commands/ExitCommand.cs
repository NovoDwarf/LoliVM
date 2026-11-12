namespace LoliVM.Example.Commands;

public sealed class ExitCommand : ITerminalCommand
{
    public string Name => "exit";
    public string Usage => "exit";

    public bool Execute(TerminalCommandContext context, string[] arguments, CancellationToken cancellationToken)
    {
        context.Session.Stop();
       
        return false;
    }
}
