using LoliVM.Example.Services;

namespace LoliVM.Example.Commands;

public sealed class EditCommand : ITerminalCommand
{
    private readonly ProgramEditor _editor;

    public EditCommand(ProgramEditor editor)
    {
        _editor = editor;
    }

    public string Name => "edit";
    public string Usage => "edit";

    public bool Execute(TerminalCommandContext context, string[] arguments, CancellationToken cancellationToken)
    {
        if (arguments.Length != 0)
        {
            context.Output.WriteLine("Usage: edit");
            return true;
        }

        _editor.Begin(context.Output);
        return true;
    }
}
