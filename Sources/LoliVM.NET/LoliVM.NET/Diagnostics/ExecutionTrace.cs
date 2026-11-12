using LoliVM.NET.Models.Snapshots;

namespace LoliVM.NET.Diagnostics;

public sealed class ExecutionTrace
{
    private readonly Queue<CpuSnapshot> _snapshots = new();
    private readonly int _capacity;

    public ExecutionTrace(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
        _capacity = capacity;
    }

    public int Count => _snapshots.Count;
    public long DroppedCount { get; private set; }

    public void Record(CpuSnapshot snapshot)
    {
        if (_snapshots.Count == _capacity)
        {
            _snapshots.Dequeue();
            DroppedCount++;
        }

        _snapshots.Enqueue(snapshot);
    }

    public CpuSnapshot[] GetSnapshots() => _snapshots.ToArray();

    public void Clear()
    {
        _snapshots.Clear();
        DroppedCount = 0;
    }
}
