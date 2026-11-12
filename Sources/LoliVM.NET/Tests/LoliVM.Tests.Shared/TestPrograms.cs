using LoliVM.NET;
using LoliVM.NET.Enums;

namespace LoliVM.Tests.Shared;

public static class TestPrograms
{
    public static MicroVm CreateMachine()
    {
        var machine = new MicroVm();
       
        machine.LoadProgram([(byte)OpCode.Mov, 0, 1, (byte)OpCode.Mov, 1, 4, (byte)OpCode.Add, 0, 1, (byte)OpCode.Halt]);
      
        return machine;
    }
}
