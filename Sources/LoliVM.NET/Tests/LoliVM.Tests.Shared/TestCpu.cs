using LoliVM.NET.Devices;
using LoliVM.NET.Devices.Buses;
using LoliVM.NET.Devices.Decoders;
using LoliVM.NET.Devices.Executors;
using LoliVM.NET.Devices.Memories;

namespace LoliVM.Tests.Shared;

public static class TestCpu
{
    public static (Cpu Cpu, Rom Rom, Ram Ram) Create(byte[] program, int romSize = 256)
    {
        var rom = new Rom(romSize);
      
        if (!rom.Load(program))
            throw new ArgumentException("Program does not fit in ROM.", nameof(program));

        var ram = new Ram(256);
        var bus = new SimpleMemoryBus();
        
        bus.Map(0, rom);
        bus.Map(0x200, ram);
        
        return (new Cpu(bus, new SimpleInstructionDecoder(), new SimpleInstructionExecutor()), rom, ram);
    }
}
