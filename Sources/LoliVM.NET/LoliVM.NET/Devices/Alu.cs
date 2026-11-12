using LoliVM.NET.Enums;
using LoliVM.NET.Models;

namespace LoliVM.NET.Devices;

public class Alu
{
    private const byte Mask = 0xFF;
    private const byte SignMask = 0x80;

    public AluResult Add(byte a, byte b)
    {
        var fullResult = a + b;
        var result = (byte)(fullResult & Mask);

        var flags = CpuFlags.None;

        flags |= CheckAddCarry(fullResult);
        flags |= CheckResultFlags(result);
        flags |= CheckAddOverflow(a, b, result);

        return new AluResult(result, flags);
    }

    public AluResult Sub(byte a, byte b)
    {
        var fullResult = a - b;
        var result = (byte)(fullResult & Mask);

        var flags = CpuFlags.None;

        flags |= CheckSubCarry(a, b);
        flags |= CheckResultFlags(result);
        flags |= CheckSubOverflow(a, b, result);

        return new AluResult(result, flags);
    }

    public AluResult And(byte a, byte b)
    {
        var result = (byte)(a & b);

        var flags = CpuFlags.None;

        flags |= CheckResultFlags(result);

        return new AluResult(result, flags);
    }

    public AluResult Or(byte a, byte b)
    {
        var result = (byte)(a | b);

        var flags = CpuFlags.None;

        flags |= CheckResultFlags(result);

        return new AluResult(result, flags);
    }

    public AluResult Xor(byte a, byte b)
    {
        var result = (byte)(a ^ b);

        var flags = CpuFlags.None;

        flags |= CheckResultFlags(result);

        return new AluResult(result, flags);
    }

    public AluResult Not(byte a)
    {
        var result = (byte)(~a);

        var flags = CpuFlags.None;

        flags |= CheckResultFlags(result);

        return new AluResult(result, flags);
    }

    private static CpuFlags CheckAddCarry(int fullResult)
    {
        return fullResult > byte.MaxValue
            ? CpuFlags.Carry
            : CpuFlags.None;
    }

    private static CpuFlags CheckSubCarry(byte a, byte b)
    {
        return a < b
            ? CpuFlags.Carry
            : CpuFlags.None;
    }

    private static CpuFlags CheckResultFlags(byte value)
    {
        return CheckZero(value) | CheckNegative(value);
    }

    private static CpuFlags CheckZero(byte value)
    {
        var zero = value == 0;

        return zero ? CpuFlags.Zero : CpuFlags.None;
    }

    private static CpuFlags CheckNegative(byte value)
    {

        var negative = (value & SignMask) != 0;

        return negative ? CpuFlags.Negative : CpuFlags.None;
    }

    private static CpuFlags CheckAddOverflow(byte a, byte b, byte value)
    {
        var signA = a & SignMask;
        var signB = b & SignMask;
        var signResult = value & SignMask;

        var overflow = signA == signB && signA != signResult;

        return overflow ? CpuFlags.Overflow : CpuFlags.None;
    }

    private static CpuFlags CheckSubOverflow(byte a, byte b, byte result)
    {
        var signA = a & SignMask;
        var signB = b & SignMask;
        var signResult = result & SignMask;

        var overflow = signA != signB && signA != signResult;

        return overflow ? CpuFlags.Overflow : CpuFlags.None;
    }
}
