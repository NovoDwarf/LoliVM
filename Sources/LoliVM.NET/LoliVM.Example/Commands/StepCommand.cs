namespace LoliVM.Example.Commands;

public sealed class StepCommand : ITerminalCommand
{
    public string Name => "step";
    public string Usage => "step";

    public bool Execute(TerminalCommandContext context, string[] arguments, CancellationToken cancellationToken)
    {
        context.Session.ExecuteSteps(1, cancellationToken);
        context.WriteStatus();
       
        return true;
    }
}
