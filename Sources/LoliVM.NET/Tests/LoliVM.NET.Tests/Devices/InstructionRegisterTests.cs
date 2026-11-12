using LoliVM.NET.Devices;
using LoliVM.NET.Enums;
using Xunit;

namespace LoliVM.NET.Tests.Devices;

public sealed class InstructionRegisterTests
{
    [Fact]
    public void Reset_RestoresNop()
    {
        // Arrange
        var register = new InstructionRegister();
        register.Load(OpCode.Halt);

        // Act
        register.Reset();

        // Assert
        Assert.Equal(OpCode.Nop, register.Value);
    }
    [Fact]
    public void Load_UpdatesValue()
    {
        // Arrange
        var register = new InstructionRegister();

        // Act
        register.Load(OpCode.Halt);

        // Assert
        Assert.Equal(OpCode.Halt, register.Value);
    }
}
