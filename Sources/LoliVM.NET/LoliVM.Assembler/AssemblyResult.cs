namespace LoliVM.Assembly;

public sealed record AssemblyResult(byte[] Bytecode, IReadOnlyList<AssemblyDiagnostic> Diagnostics)
{
    public bool Success => Diagnostics.Count == 0;
}
