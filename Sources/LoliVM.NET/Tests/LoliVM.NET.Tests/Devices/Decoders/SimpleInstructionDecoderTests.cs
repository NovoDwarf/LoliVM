using LoliVM.NET.Devices.Decoders;
using LoliVM.NET.Enums;
using Xunit;

namespace LoliVM.NET.Tests.Devices.Decoders;

public sealed class SimpleInstructionDecoderTests
{
    public static TheoryData<OpCode, OperandKind[]> Instructions => new()
    {
        { OpCode.Nop, [OperandKind.None] },
        { OpCode.Mov, [OperandKind.Register, OperandKind.Immediate8] },
        { OpCode.Add, [OperandKind.Register, OperandKind.Register] },
        { OpCode.Sub, [OperandKind.Register, OperandKind.Register] },
        { OpCode.Load, [OperandKind.Register, OperandKind.Address16] },
        { OpCode.Store, [OperandKind.Address16, OperandKind.Register] },
        { OpCode.Jump, [OperandKind.Address16] },
        { OpCode.Halt, [OperandKind.None] }
    };

    [Theory]
    [MemberData(nameof(Instructions))]
    public void Decode_KnownOpcodeReturnsExpectedOperands(OpCode opcode, OperandKind[] operands)
    {
        // Arrange
        var decoder = new SimpleInstructionDecoder();

        // Act
        var success = decoder.TryDecode(opcode, out var descriptor);

        // Assert
        Assert.True(success);
        Assert.Equal(opcode, descriptor.OpCode);
        Assert.Equal(operands, descriptor.Operands);
    }

    [Fact]
    public void Decode_UnknownOpcodeFails()
    {
        // Arrange
        var decoder = new SimpleInstructionDecoder();

        // Act
        var success = decoder.TryDecode((OpCode)0xFE, out _);

        // Assert
        Assert.False(success);
    }
}
