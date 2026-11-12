using System.Collections.Immutable;
﻿using LoliVM.NET.Interfaces;
using LoliVM.NET.Models;
using LoliVM.NET.Models.Snapshots;

namespace LoliVM.NET.Devices.Buses;

public sealed class SimpleMemoryBus : IMemoryBus
{
    private readonly List<MemoryMapping> _mappings = [];

    public void Map(ushort startAddress, IMemoryDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        if (device.Size <= 0)
            throw new ArgumentOutOfRangeException(nameof(device));

        var start = (int)startAddress;
        var end = start + (long)device.Size - 1;

        if (end > ushort.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(startAddress), "Device exceeds address space.");

        foreach (var mapping in _mappings)
        {
            if (start <= mapping.End && mapping.Start <= end)
                throw new InvalidOperationException("Memory regions overlap.");
        }

        _mappings.Add(new MemoryMapping(start, (int)end, device));
    }

    public bool TryRead(ushort address, out byte value)
    {
        foreach (var mapping in _mappings)
        {
            if (address < mapping.Start || address > mapping.End)
                continue;

            var offset = address - mapping.Start;
            return mapping.Device.TryRead(offset, out value);
        }

        value = 0;
        return false;
    }

    public bool TryWrite(ushort address, byte value)
    {
        foreach (var mapping in _mappings)
        {
            if (address < mapping.Start || address > mapping.End)
                continue;

            var offset = address - mapping.Start;
            return mapping.Device.TryWrite(offset, value);
        }

        return false;
    }

    public MemoryBusSnapshot Dump()
    {
        var mapping = _mappings
                      .Select(mapping => new MemoryMappingSnapshot(mapping.Start, mapping.End, new MemoryDeviceSnapshot(mapping.Device.ToString() ?? mapping.Device.GetType().Name)))
                      .ToImmutableArray();

        return new MemoryBusSnapshot(mapping);
    }
}
