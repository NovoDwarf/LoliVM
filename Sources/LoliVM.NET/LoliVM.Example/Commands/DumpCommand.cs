using LoliVM.Example.Utilities;

namespace LoliVM.Example.Commands;

public sealed class DumpCommand : ITerminalCommand
{
    public string Name => "dump";
    public string Usage => "dump";

    public bool Execute(TerminalCommandContext context, string[] arguments, CancellationToken cancellationToken)
    {
        var trace = context.Session.Trace;
     
        context.Output.WriteLine($"Trace: {trace.Count} entries, {trace.DroppedCount} dropped.");
     
        foreach (var snapshot in trace.GetSnapshots()) 
            context.Output.WriteLine(CpuDumpFormatter.Format(snapshot));
      
        return true;
    }
}
