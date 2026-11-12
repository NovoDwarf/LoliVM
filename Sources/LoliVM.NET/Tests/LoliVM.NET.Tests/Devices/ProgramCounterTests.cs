using LoliVM.NET.Devices;
using Xunit;

namespace LoliVM.NET.Tests.Devices;

public sealed class ProgramCounterTests
{
    [Fact]
    public void Advance_WrapsAtEndOfAddressSpace()
    {
        // Arrange
        var counter = new ProgramCounter();
        counter.Jump(ushort.MaxValue);

        // Act
        counter.Advance();

        // Assert
        Assert.Equal(0, counter.Value);
    }
    [Fact]
    public void Reset_ReturnsCounterToZero()
    {
        // Arrange
        var counter = new ProgramCounter();
        counter.Jump(10);
        var snapshot = counter.Dump();

        // Act
        counter.Reset();

        // Assert
        Assert.Equal(10, snapshot.Value);
        Assert.Equal(0, counter.Value);
    }
}
