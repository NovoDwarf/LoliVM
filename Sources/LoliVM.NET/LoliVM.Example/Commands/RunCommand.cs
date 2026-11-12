namespace LoliVM.Example.Commands;

public sealed class RunCommand : ITerminalCommand
{
    public string Name => "run";
    public string Usage => "run [limit]";

    public bool Execute(TerminalCommandContext context, string[] arguments, CancellationToken cancellationToken)
    {
        var count = 1000;
      
        if (arguments.Length > 1 || (arguments.Length == 1 && (!int.TryParse(arguments[0], out count) || count <= 0)))
        {
            context.Output.WriteLine("Usage: run [positive instruction limit]");
            return true;
        }

        context.Output.WriteLine($"Executed {context.Session.ExecuteSteps(count, cancellationToken)} step(s).");
        context.WriteStatus();
        
        return true;
    }
}
