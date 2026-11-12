using LoliVM.NET.Devices.Buses;
using LoliVM.NET.Devices.Memories;
using Xunit;

namespace LoliVM.NET.Tests.Devices.Buses;

public sealed class SimpleMemoryBusTests
{
    [Fact]
    public void MappedAccess_UsesDeviceOffsetsAndRejectsUnmappedAddresses()
    {
        // Arrange
        var bus = new SimpleMemoryBus();
        var ram = new Ram(2);
        bus.Map(0x200, ram);

        // Act
        var written = bus.TryWrite(0x201, 42);
        var read = ram.TryRead(1, out var value);
        var unmappedRead = bus.TryRead(0x202, out _);
        var unmappedWrite = bus.TryWrite(0x1FF, 1);

        // Assert
        Assert.True(written);
        Assert.True(read);
        Assert.Equal(42, value);
        Assert.False(unmappedRead);
        Assert.False(unmappedWrite);
    }

    [Fact]
    public void Map_RejectsOverlapAndAddressSpaceOverflow()
    {
        // Arrange
        var bus = new SimpleMemoryBus();
        bus.Map(10, new Ram(4));

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => bus.Map(13, new Ram(2)));
        Assert.Throws<ArgumentOutOfRangeException>(() => bus.Map(ushort.MaxValue, new Ram(2)));
    }

    [Fact]
    public void WriteToRom_IsRejected()
    {
        // Arrange
        var bus = new SimpleMemoryBus();
        bus.Map(0, new Rom(1));

        // Act
        var written = bus.TryWrite(0, 1);

        // Assert
        Assert.False(written);
    }
    [Fact]
    public void Map_AllowsAdjacentRegions()
    {
        // Arrange
        var bus = new SimpleMemoryBus();
        bus.Map(10, new Ram(4));

        // Act
        bus.Map(14, new Ram(1));
        var snapshot = bus.Dump();

        // Assert
        Assert.Equal(2, snapshot.Mappings.Length);
    }
}
