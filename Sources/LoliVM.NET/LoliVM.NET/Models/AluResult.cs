using LoliVM.NET.Enums;

namespace LoliVM.NET.Models;

public readonly record struct AluResult(
	byte Value,
	CpuFlags Flags
	);