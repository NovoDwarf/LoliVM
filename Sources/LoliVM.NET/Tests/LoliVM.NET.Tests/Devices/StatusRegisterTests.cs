using LoliVM.NET.Devices;
using LoliVM.NET.Enums;
using Xunit;

namespace LoliVM.NET.Tests.Devices;

public sealed class StatusRegisterTests
{
    [Fact]
    public void SetClearAndUpdate_ManageFlags()
    {
        // Arrange
        var status = new StatusRegister();
        status.Set(CpuFlags.Carry | CpuFlags.Zero);

        // Act
        status.Clear(CpuFlags.Carry);
        var afterClear = status.Dump();
        status.Update(CpuFlags.Negative);
        var afterUpdate = status.Flags;
        status.Reset();

        // Assert
        Assert.Equal(CpuFlags.Zero, afterClear.Flags);
        Assert.Equal(CpuFlags.Negative, afterUpdate);
        Assert.Equal(CpuFlags.None, status.Flags);
    }
    [Fact]
    public void Set_AllowsTestingFlags()
    {
        // Arrange
        var status = new StatusRegister();

        // Act
        status.Set(CpuFlags.Carry | CpuFlags.Zero);
        var hasAll = status.HasAll(CpuFlags.Carry | CpuFlags.Zero);
        var hasNegative = status.HasAny(CpuFlags.Negative);

        // Assert
        Assert.True(hasAll);
        Assert.False(hasNegative);
    }
}
