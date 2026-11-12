using System.Globalization;
using LoliVM.NET.Devices.Decoders;
using LoliVM.NET.Enums;

namespace LoliVM.Assembly;

public sealed class Assembler
{
    private static readonly Dictionary<string, OpCode> Opcodes = Enum.GetValues<OpCode>()
        .ToDictionary(opcode => opcode.ToString(), StringComparer.OrdinalIgnoreCase);
    private readonly SimpleInstructionDecoder _decoder = new();

    public AssemblyResult Assemble(string source, int maxProgramSize = 256)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxProgramSize);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(maxProgramSize, ushort.MaxValue + 1);
       
        var diagnostics = new List<AssemblyDiagnostic>();
        var labels = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var instructions = new List<SourceInstruction>();
        var address = 0;
        var lines = source.Split('\n');

        for (var index = 0; index < lines.Length; index++)
        {
            var lineNumber = index + 1;
            var text = lines[index].Split(';', 2)[0].Trim();
           
            if (text.Length == 0)
                continue;

            var colon = text.IndexOf(':');
           
            if (colon >= 0)
            {
                var label = text[..colon].Trim();
             
                if (!IsIdentifier(label))
                {
                    diagnostics.Add(new AssemblyDiagnostic(lineNumber, $"Invalid label '{label}'."));
                    continue;
                }

                if (!labels.TryAdd(label, address)) 
                    diagnostics.Add(new AssemblyDiagnostic(lineNumber, $"Duplicate label '{label}'."));

                text = text[(colon + 1)..].Trim();
               
                if (text.Length == 0)
                    continue;
            }

            var separator = 0;
           
            while (separator < text.Length && !char.IsWhiteSpace(text[separator])) 
                separator++;

            var mnemonic = text[..separator];
            
            if (!Opcodes.TryGetValue(mnemonic, out var opcode) || !_decoder.TryDecode(opcode, out var descriptor))
            {
                diagnostics.Add(new AssemblyDiagnostic(lineNumber, $"Unknown instruction '{mnemonic}'."));
                continue;
            }

            var operandText = text[separator..].Trim();
            var operands = operandText.Length == 0 ? [] : operandText.Split(',', StringSplitOptions.TrimEntries);
            var kinds = descriptor.Operands.Where(kind => kind != OperandKind.None).ToArray();
          
            if (operands.Length != kinds.Length || operands.Any(string.IsNullOrEmpty))
            {
                diagnostics.Add(new AssemblyDiagnostic(lineNumber, $"{mnemonic} expects {kinds.Length} operand(s), separated by commas."));
                continue;
            }

            instructions.Add(new SourceInstruction(lineNumber, opcode, operands, kinds));
            address += 1 + kinds.Sum(kind => kind == OperandKind.Address16 ? 2 : 1);
            
            if (address <= maxProgramSize)
                continue;
            
            diagnostics.Add(new AssemblyDiagnostic(lineNumber, $"Program exceeds {maxProgramSize} bytes."));
            break;
        }

        if (instructions.Count == 0 && diagnostics.Count == 0)
        {
            diagnostics.Add(new AssemblyDiagnostic(1, "Program must contain at least one instruction."));
        }

        var bytecode = new List<byte>(address);
       
        foreach (var instruction in instructions)
        {
            bytecode.Add((byte)instruction.OpCode);
           
            for (var index = 0; index < instruction.Operands.Length; index++)
            {
                var token = instruction.Operands[index];
                var kind = instruction.Kinds[index];
               
                if (!TryResolveOperand(token, kind, labels, out var value))
                {
                    diagnostics.Add(new AssemblyDiagnostic(instruction.Line, $"Invalid {kind} operand '{token}'."));
                    continue;
                }

                bytecode.Add((byte)(value & 0xFF));
                if (kind == OperandKind.Address16)
                {
                    bytecode.Add((byte)(value >> 8));
                }
            }
        }

        return new AssemblyResult(diagnostics.Count == 0 ? [.. bytecode] : [], diagnostics.AsReadOnly());
    }

    private static bool TryResolveOperand(string token, OperandKind kind, Dictionary<string, int> labels, out ushort value)
    {
        value = 0;
        switch (kind)
        {
            case OperandKind.Register:
            {
                if (token.Length != 2 || char.ToUpperInvariant(token[0]) != 'R' || token[1] is < '0' or > '3')
                {
                    return false;
                }

                value = (ushort)(token[1] - '0');
                return true;
            }
            case OperandKind.Address16 when labels.TryGetValue(token, out var address):
            {
                if (address > ushort.MaxValue)
                {
                    return false;
                }

                value = (ushort)address;
                return true;
            }
        }

        var hexadecimal = token.StartsWith("0x", StringComparison.OrdinalIgnoreCase);
        var digits = hexadecimal ? token[2..] : token;
        var style = hexadecimal ? NumberStyles.AllowHexSpecifier : NumberStyles.None;
      
        return ushort.TryParse(digits, style, CultureInfo.InvariantCulture, out value)
            && (kind == OperandKind.Address16 || value <= byte.MaxValue);
    }

    private static bool IsIdentifier(string text)
    {
        return text.Length > 0
            && (char.IsAsciiLetter(text[0]) || text[0] == '_')
            && text.All(character => char.IsAsciiLetterOrDigit(character) || character == '_');
    }

    private sealed record SourceInstruction(int Line, OpCode OpCode, string[] Operands, OperandKind[] Kinds);
}
