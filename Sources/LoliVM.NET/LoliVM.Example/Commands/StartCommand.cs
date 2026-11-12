namespace LoliVM.Example.Commands;

public sealed class StartCommand : ITerminalCommand
{
    public string Name => "start";
    public string Usage => "start";

    public bool Execute(TerminalCommandContext context, string[] arguments, CancellationToken cancellationToken)
    {
        context.Session.Start();
        context.Output.WriteLine("Machine started.");
      
        return true;
    }
}
