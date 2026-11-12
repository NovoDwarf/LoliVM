using LoliVM.NET.Interfaces;

namespace LoliVM.NET.Models;

public readonly record struct MemoryMapping(
	int Start,
	int End,
	IMemoryDevice Device
);