using LoliVM.NET.Devices;
using Xunit;

namespace LoliVM.NET.Tests.Devices;

public sealed class RegisterFileTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_RejectsInvalidCount(int count)
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new RegisterFile(count));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void Access_RejectsOutOfRangeIndex(int index)
    {
        // Arrange
        var registers = new RegisterFile(2);

        // Act
        var written = registers.TrySet(index, 42);
        var read = registers.TryGet(index, out var value);

        // Assert
        Assert.False(written);
        Assert.False(read);
        Assert.Equal(0, value);
    }

    [Fact]
    public void Reset_ClearsRegistersWithoutChangingEarlierSnapshot()
    {
        // Arrange
        var registers = new RegisterFile(2);
        registers.TrySet(1, 42);
        var snapshot = registers.Dump();

        // Act
        registers.Reset();
        var read = registers.TryGet(1, out var value);

        // Assert
        Assert.True(read);
        Assert.Equal(0, value);
        Assert.Equal(42, snapshot.Registers[1]);
        Assert.Equal(2, snapshot.Count);
    }
}
