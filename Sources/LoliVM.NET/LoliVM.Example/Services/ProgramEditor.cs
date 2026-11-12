using LoliVM.Assembly;
using LoliVM.Example.Models;

namespace LoliVM.Example.Services;

public sealed class ProgramEditor
{
    private readonly Assembler _assembler;
    private readonly MachineSession _session;
    private readonly List<string> _lines = [];

    public ProgramEditor(Assembler assembler, MachineSession session)
    {
        _assembler = assembler;
        _session = session;
    }

    public bool Active { get; private set; }
    public string Prompt => $"asm[{_lines.Count + 1}]> ";

    public void Begin(TextWriter output)
    {
        if (_session.Machine.Running)
        {
            output.WriteLine("Stop the machine before editing a program.");
            return;
        }

        Discard();
        Active = true;
        output.WriteLine("Assembly input: .load | .list | .undo | .clear | .cancel | .help");
    }

    public void ProcessLine(string line, TextWriter output)
    {
        switch (line.Trim().ToLowerInvariant())
        {
            case ".load":
                Load(output);
                return;
            case ".list":
                for (var index = 0; index < _lines.Count; index++)
                {
                    output.WriteLine($"{index + 1}: {_lines[index]}");
                }
                return;
            case ".undo":
                if (_lines.Count > 0)
                {
                    _lines.RemoveAt(_lines.Count - 1);
                }
                return;
            case ".clear":
                _lines.Clear();
                return;
            case ".cancel":
                Discard();
                output.WriteLine("Program discarded.");
                return;
            case ".help":
                output.WriteLine("MOV R0, 5 | ADD R0, R1 | SUB R0, R1 | LOAD R0, 0x0200");
                output.WriteLine("STORE 0x0200, R0 | JUMP label | NOP | HALT | label: | ; comment");
                output.WriteLine(".load assembles and loads; .list shows source; .undo removes last line; .clear clears source; .cancel discards edits.");
                return;
        }

        if (line.TrimStart().StartsWith('.'))
        {
            output.WriteLine("Unknown editor command. Type .help.");
            return;
        }

        _lines.Add(line);
    }

    public void Discard()
    {
        _lines.Clear();
        Active = false;
    }

    private void Load(TextWriter output)
    {
        if (_session.Machine.Running)
        {
            output.WriteLine("Stop the machine before loading a program.");
            return;
        }

        var result = _assembler.Assemble(string.Join('\n', _lines));
       
        if (!result.Success)
        {
            foreach (var diagnostic in result.Diagnostics) 
                output.WriteLine($"Line {diagnostic.Line}: {diagnostic.Message}");
            
            return;
        }

        if (!_session.LoadProgram(result.Bytecode))
        {
            output.WriteLine("Program does not fit in ROM.");
            return;
        }

        Discard();
        output.WriteLine($"Loaded {result.Bytecode.Length} bytes. Use start, then step or run.");
    }
}
