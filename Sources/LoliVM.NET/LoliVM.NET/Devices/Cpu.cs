using LoliVM.NET.Enums;
using LoliVM.NET.Interfaces;
using LoliVM.NET.Models;
using LoliVM.NET.Models.Snapshots;

namespace LoliVM.NET.Devices;

public class Cpu
{
    private readonly IMemoryBus _bus;
    private readonly IInstructionDecoder _decoder;
    private readonly IInstructionExecutor _executor;

    private readonly Alu _alu;
    private readonly StatusRegister _statusRegister;
    private readonly RegisterFile _registerFile;
    private readonly ProgramCounter _pc;
    private readonly InstructionRegister _ir;

    private readonly CpuContext _context;

    public Cpu(IMemoryBus bus, IInstructionDecoder decoder, IInstructionExecutor executor)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(decoder);
        ArgumentNullException.ThrowIfNull(executor);

        _bus = bus;
        _decoder = decoder;
        _executor = executor;

        _alu = new Alu();
        _statusRegister = new StatusRegister();
        _registerFile = new RegisterFile(4);

        _pc = new ProgramCounter();
        _ir = new InstructionRegister();

        _context = new CpuContext
        {
            Registers = _registerFile,
            Status = _statusRegister,
            Pc = _pc,
            Bus = _bus,
            Alu = _alu
        };
    }

    public bool Halted => _context.Halted;
    public bool Faulted => _context.Faulted;

    public InstructionResult? LastResult { get; private set; }

    public InstructionResult? Step()
    {
        if (Halted || Faulted)
        {
            return null;
        }

        var result = !TryFetchInstruction(out var instruction, out var fetchStatus)
            ? new InstructionResult(instruction, fetchStatus) 
            : _executor.TryExecute(instruction, _context);

        if (result.Status == ExecutionStatus.Halted)
        {
            _context.Halt();
        }
        else if (result.Status != ExecutionStatus.Success)
        {
            _context.Fault();
        }

        LastResult = result;
        return result;
    }

    public CpuSnapshot CaptureSnapshot()
    {
        return new CpuSnapshot(LastResult, _registerFile.Dump(), _statusRegister.Dump(), _pc.Dump(), _bus.Dump())
        {
            Halted = Halted,
            Faulted = Faulted
        };
    }
    
    public void Reset()
    {
        _registerFile.Reset();
        _statusRegister.Reset();
        _pc.Reset();
        _ir.Reset();
        _context.Reset();
        
        LastResult = null;
    }

    internal bool TryFetchInstruction(out Instruction instruction)
    {
        return TryFetchInstruction(out instruction, out _);
    }

    private bool TryFetchInstruction(out Instruction instruction, out ExecutionStatus fetchStatus)
    {
        instruction = default;
        fetchStatus = ExecutionStatus.MemoryAccessFault;

        if (!TryFetchByte(out var rawOpcode))
            return false;

        _ir.Load((OpCode)rawOpcode);

        instruction = new Instruction(_ir.Value, 0, 0);
        fetchStatus = ExecutionStatus.InvalidInstruction;
      
        if (!_decoder.TryDecode(_ir.Value, out var descriptor))
            return false;
        
        var operand0 = (ushort)0;
        var operand1 = (ushort)0;
        var kind = descriptor.Operands;

        if (kind.Length is > 2 or <= 0)
            return false;

        fetchStatus = ExecutionStatus.MemoryAccessFault;
        if (!TryFetchValue(kind[0], out operand0))
            return false;

        if (kind.Length == 2 && !TryFetchValue(kind[1], out operand1))
            return false;

        instruction = new Instruction(_ir.Value, operand0, operand1);
        return true;
    }

    private bool TryFetchWord(out ushort value)
    {
        value = 0;

        if (!TryFetchByte(out var low))
            return false;

        if (!TryFetchByte(out var high))
            return false;

        value = (ushort)(low | (high << 8));
        return true;
    }

    private bool TryFetchByte(out byte value)
    {
        if (!_bus.TryRead(_pc.Value, out value))
            return false;

        _pc.Advance();
        return true;
    }

    private bool TryFetchValue(OperandKind kind, out ushort value)
    {
        value = 0;

        switch (kind)
        {
            case OperandKind.None: return true;
            case OperandKind.Register:
            case OperandKind.Immediate8:
                var result = TryFetchByte(out var bValue);
                value = bValue;
                return result;
            case OperandKind.Address16: return TryFetchWord(out value);
            default: return false;
        }
    }
}
