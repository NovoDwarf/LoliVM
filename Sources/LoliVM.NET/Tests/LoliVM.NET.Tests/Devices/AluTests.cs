using LoliVM.NET.Devices;
using LoliVM.NET.Enums;
using LoliVM.NET.Models;
using Xunit;

namespace LoliVM.NET.Tests.Devices;

public sealed class AluTests
{
    private readonly Alu _alu = new();

    [Theory]
    [InlineData(255, 1, 0, CpuFlags.Zero | CpuFlags.Carry)]
    [InlineData(127, 1, 128, CpuFlags.Negative | CpuFlags.Overflow)]
    [InlineData(128, 128, 0, CpuFlags.Zero | CpuFlags.Carry | CpuFlags.Overflow)]
    [InlineData(1, 2, 3, CpuFlags.None)]
    public void Add_ReturnsWrappedResultAndFlags(byte left, byte right, byte expected, CpuFlags flags)
    {
        // Arrange
        var expectedResult = new AluResult(expected, flags);

        // Act
        var result = _alu.Add(left, right);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData(0, 1, 255, CpuFlags.Carry | CpuFlags.Negative)]
    [InlineData(128, 1, 127, CpuFlags.Overflow)]
    [InlineData(3, 3, 0, CpuFlags.Zero)]
    [InlineData(5, 2, 3, CpuFlags.None)]
    public void Sub_ReturnsWrappedResultAndFlags(byte left, byte right, byte expected, CpuFlags flags)
    {
        // Arrange
        var expectedResult = new AluResult(expected, flags);

        // Act
        var result = _alu.Sub(left, right);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void LogicalOperations_SetResultFlags()
    {
        // Arrange
        var alu = new Alu();

        // Act
        var and = alu.And(1, 2);
        var or = alu.Or(128, 0);
        var xor = alu.Xor(1, 2);
        var not = alu.Not(0);

        // Assert
        Assert.Equal(new AluResult(0, CpuFlags.Zero), and);
        Assert.Equal(new AluResult(128, CpuFlags.Negative), or);
        Assert.Equal(new AluResult(3, CpuFlags.None), xor);
        Assert.Equal(new AluResult(255, CpuFlags.Negative), not);
    }
}
