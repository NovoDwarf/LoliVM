namespace LoliVM.Example.Commands;

public interface ITerminalCommand
{
    public string Name { get; }
    public string Usage { get; }

    public bool Execute(TerminalCommandContext context, string[] arguments, CancellationToken cancellationToken);
}
