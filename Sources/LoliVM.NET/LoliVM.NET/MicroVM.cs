using LoliVM.NET.Devices;
using LoliVM.NET.Devices.Buses;
using LoliVM.NET.Devices.Decoders;
using LoliVM.NET.Devices.Executors;
using LoliVM.NET.Devices.Memories;
using LoliVM.NET.Models;
using LoliVM.NET.Models.Snapshots;

namespace LoliVM.NET;

public class MicroVm
{
    private readonly Cpu _cpu;
    private readonly Ram _ram;
    private readonly Rom _rom;

    public MicroVm()
    {
        _ram = new Ram(256);
        _rom = new Rom(256);
       
        var bus = new SimpleMemoryBus();
        
        bus.Map(0x000, _rom);
        bus.Map(0x200, _ram);
      
        _cpu = new Cpu(bus, new SimpleInstructionDecoder(), new SimpleInstructionExecutor());
    }

    public bool Running { get; private set; }
    public bool Halted => _cpu.Halted;
    public bool Faulted => _cpu.Faulted;

    public bool LoadProgram(ReadOnlySpan<byte> program)
    {
        if (Running)
            throw new InvalidOperationException("Stop the machine before loading firmware.");

        return _rom.Load(program);
    }

    public void Start()
    {
        if (Running)
        {
            return;
        }

        Reset();
        Running = true;
    }

    public void Reset()
    {
        _ram.Reset();
        _cpu.Reset();
    }

    public void Stop()
    {
        Running = false;
    }

    public InstructionResult? Step()
    {
        if (!Running || Halted || Faulted)
            return null;

        return _cpu.Step();
    }

    public int Run(int maxInstructions)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxInstructions);
       
        var executed = 0;
        
        while (Running && !Halted && !Faulted && executed < maxInstructions)
        {
            Step();
            executed++;
        }

        return executed;
    }

    public CpuSnapshot CaptureSnapshot() => _cpu.CaptureSnapshot();
}
