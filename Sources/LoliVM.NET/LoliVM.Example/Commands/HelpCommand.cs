namespace LoliVM.Example.Commands;

public sealed class HelpCommand : ITerminalCommand
{
    public string Name => "help";
    public string Usage => "help";

    public bool Execute(TerminalCommandContext context, string[] arguments, CancellationToken cancellationToken)
    {
        context.Output.WriteLine(string.Join(" | ", context.Commands.Select(command => command.Usage)));
     
        return true;
    }
}
