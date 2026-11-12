using LoliVM.NET.Devices.Memories;
using Xunit;

namespace LoliVM.NET.Tests.Devices.Memories;

public sealed class RomTests
{
    [Fact]
    public void Load_ReplacesImageAndClearsRemainingBytes()
    {
        // Arrange
        var rom = new Rom(3);
        rom.Load([1, 2, 3]);

        // Act
        var loaded = rom.Load([4]);
        var firstRead = rom.TryRead(0, out var first);
        var secondRead = rom.TryRead(1, out var second);

        // Assert
        Assert.True(loaded);
        Assert.True(firstRead);
        Assert.True(secondRead);
        Assert.Equal(4, first);
        Assert.Equal(0, second);
    }

    [Fact]
    public void InvalidLoadAndReset_PreserveImageAndWritesAreRejected()
    {
        // Arrange
        var rom = new Rom(2);
        rom.Load([1]);

        // Act
        var loaded = rom.Load([2, 3, 4]);
        var written = rom.TryWrite(0, 2);
        rom.Reset();
        var read = rom.TryRead(0, out var value);
        var readBeforeStart = rom.TryRead(-1, out _);
        var readAfterEnd = rom.TryRead(2, out _);

        // Assert
        Assert.False(loaded);
        Assert.False(written);
        Assert.True(read);
        Assert.Equal(1, value);
        Assert.False(readBeforeStart);
        Assert.False(readAfterEnd);
    }

    [Fact]
    public void Constructor_RejectsEmptyMemory()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new Rom(0));
    }
}
