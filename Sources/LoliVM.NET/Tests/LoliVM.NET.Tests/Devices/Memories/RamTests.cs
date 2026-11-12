using LoliVM.NET.Devices.Memories;
using Xunit;

namespace LoliVM.NET.Tests.Devices.Memories;

public sealed class RamTests
{
    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    public void Access_RejectsInvalidOffsets(int offset)
    {
        // Arrange
        var ram = new Ram(4);

        // Act
        var written = ram.TryWrite(offset, 1);
        var read = ram.TryRead(offset, out var value);

        // Assert
        Assert.False(written);
        Assert.False(read);
        Assert.Equal(0, value);
    }

    [Fact]
    public void Reset_ClearsWrittenData()
    {
        // Arrange
        var ram = new Ram(4);
        ram.TryWrite(3, 42);

        // Act
        ram.Reset();
        var read = ram.TryRead(3, out var value);

        // Assert
        Assert.True(read);
        Assert.Equal(0, value);
    }

    [Fact]
    public void Constructor_RejectsEmptyMemory()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new Ram(0));
    }
}
