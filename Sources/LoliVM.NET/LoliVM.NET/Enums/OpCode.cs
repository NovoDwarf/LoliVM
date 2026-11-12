namespace LoliVM.NET.Enums;

public enum OpCode : byte
{
	Nop = 0x00,
	Mov = 0x01,
	Add = 0x02,
	Sub = 0x03,
	Load = 0x04,
	Store = 0x05,
	Jump = 0x06,
	Halt = 0xFF
}