using VmAssembler = LoliVM.Assembly.Assembler;
using Xunit;

namespace LoliVM.Assembler.Tests;

public sealed class AssemblerTests
{
    private readonly VmAssembler _assembler = new();

    public static TheoryData<string, byte[]> InstructionCases => new()
    {
        { "NOP", [0x00] },
        { "MOV R3, 255", [0x01, 3, 255] },
        { "ADD R0, R1", [0x02, 0, 1] },
        { "SUB R2, R3", [0x03, 2, 3] },
        { "LOAD R0, 0x1234", [0x04, 0, 0x34, 0x12] },
        { "STORE 0x0200, R1", [0x05, 0, 2, 1] },
        { "JUMP 65535", [0x06, 255, 255] },
        { "HALT", [0xFF] },
        { "\tmov\tr0, 0XfF ; comment\r\n", [0x01, 0, 255] }
    };

    [Theory]
    [MemberData(nameof(InstructionCases))]
    public void Assemble_EncodesExistingInstructionSet(string source, byte[] expected)
    {
        // Arrange
        var assembler = _assembler;

        // Act
        var result = assembler.Assemble(source);

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.Equal(expected, result.Bytecode);
    }

    [Fact]
    public void Assemble_ResolvesForwardAndBackwardLabelsAsByteAddresses()
    {
        // Arrange
        const string source = "JUMP End\nloop: MOV R0, 1\nJUMP LOOP\nend: HALT";

        // Act
        var result = _assembler.Assemble(source);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(new byte[] { 6, 9, 0, 1, 0, 1, 6, 3, 0, 255 }, result.Bytecode);
    }

    [Theory]
    [InlineData("", 1)]
    [InlineData("; only comment", 1)]
    [InlineData("NOP\nwat", 2)]
    [InlineData("MOV R4, 1", 1)]
    [InlineData("MOV R0, 256", 1)]
    [InlineData("MOV R0, -1", 1)]
    [InlineData("LOAD R0, 65536", 1)]
    [InlineData("JUMP missing", 1)]
    [InlineData("MOV R0 1", 1)]
    [InlineData("MOV R0,,1", 1)]
    [InlineData("HALT 1", 1)]
    [InlineData("1: NOP", 1)]
    [InlineData("label: NOP\nLABEL: HALT", 2)]
    [InlineData("JUMP -1", 1)]
    [InlineData("MOV R0, 0x", 1)]
    public void Assemble_InvalidSourceReportsLineWithoutPartialBytecode(string source, int errorLine)
    {
        // Arrange
        var assembler = _assembler;

        // Act
        var result = assembler.Assemble(source);

        // Assert
        Assert.False(result.Success);
        Assert.Empty(result.Bytecode);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Line == errorLine);
    }

    [Fact]
    public void Assemble_RejectsRomOverflowButAcceptsExactCapacity()
    {
        // Arrange
        var exact = string.Join('\n', Enumerable.Repeat("NOP", 256));
        var overflow = exact + "\nHALT";

        // Act
        var valid = _assembler.Assemble(exact);
        var invalid = _assembler.Assemble(overflow);

        // Assert
        Assert.True(valid.Success);
        Assert.Equal(256, valid.Bytecode.Length);
        Assert.False(invalid.Success);
        Assert.Empty(invalid.Bytecode);
        Assert.Contains(invalid.Diagnostics, diagnostic => diagnostic.Line == 257);
    }
}
