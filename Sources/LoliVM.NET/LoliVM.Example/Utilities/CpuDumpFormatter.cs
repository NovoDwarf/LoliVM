using System.Text;
using LoliVM.NET.Models.Snapshots;

namespace LoliVM.Example.Utilities;

public static class CpuDumpFormatter
{
    public static string Format(CpuSnapshot snapshot)
    {
        var builder = new StringBuilder();

        builder.AppendLine("--- ### [CPU DUMP] ### ---");

        builder.AppendLine("### [CURRENT STATUS] ###");
        builder.AppendLine($"---> Halted: {snapshot.Halted}");
        builder.AppendLine($"---> Faulted: {snapshot.Faulted}");
        builder.AppendLine();

        builder.AppendLine("### [CURRENT INSTRUCTION] ###");
        builder.AppendLine($"---> Operation Code: {snapshot.Result?.Instruction.OpCode}");
        builder.AppendLine($"---> Operand 0: {snapshot.Result?.Instruction.Operand0}");
        builder.AppendLine($"---> Operand 1: {snapshot.Result?.Instruction.Operand1}");
        builder.AppendLine($"---> Result: {snapshot.Result?.Status}");
        builder.AppendLine();

        builder.AppendLine("### [REGISTERS] ###");

        for (var index = 0; index < snapshot.Registers.Registers.Length; index++)
            builder.AppendLine($"---> Register [R{index}]: {snapshot.Registers.Registers[index]}");
        builder.AppendLine();

        builder.AppendLine("### [STATUS REGISTER] ###");
        builder.AppendLine($"---> CPU Flags: {snapshot.Status.Flags}");
        builder.AppendLine();

        builder.AppendLine("### [PROGRAM COUNTER] ###");
        builder.AppendLine($"---> PC: {snapshot.ProgramCounter.Value:X4}");
        builder.AppendLine();

        builder.AppendLine("### [MEMORY BUS] ###");
        for (var index = 0; index < snapshot.MemoryBus.Mappings.Length; index++)
        {
            var mapping = snapshot.MemoryBus.Mappings[index];

            builder.AppendLine($"--> Device [{index}]: {mapping.Device.Name}");
            builder.AppendLine($"- Start: {mapping.Start:X4}");
            builder.AppendLine($"- End: {mapping.End:X4}");
        }

        return builder.ToString();
    }
}
