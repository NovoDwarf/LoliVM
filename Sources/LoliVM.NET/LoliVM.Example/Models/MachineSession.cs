using LoliVM.NET;
using LoliVM.NET.Diagnostics;
using LoliVM.NET.Enums;
using Microsoft.Extensions.Logging;

namespace LoliVM.Example.Models;

public sealed class MachineSession
{
    private readonly ILogger<MachineSession> _logger;

    public MachineSession(MicroVm machine, ILogger<MachineSession> logger)
    {
        ArgumentNullException.ThrowIfNull(machine);
        ArgumentNullException.ThrowIfNull(logger);
       
        Machine = machine;
        _logger = logger;
    }

    public MicroVm Machine { get; }

    public ExecutionTrace Trace { get; } = new(1000);

    private long _stepNumber;
    
    public bool LoadProgram(ReadOnlySpan<byte> program)
    {
        if (!Machine.LoadProgram(program))
        {
            return false;
        }

        Machine.Reset();
        ClearTrace();
        _logger.LogInformation("Firmware loaded: {ProgramSize} bytes", program.Length);
        return true;
    }

    public void Start()
    {
        if (Machine.Running)
            return;

        Machine.Start();
        ClearTrace();
        _logger.LogInformation("Machine started");
    }

    public void Reset()
    {
        Machine.Reset();
        ClearTrace();
        _logger.LogInformation("Machine reset; running: {Running}", Machine.Running);
    }

    public void Stop()
    {
        if (!Machine.Running)
            return;

        Machine.Stop();
        _logger.LogInformation("Machine stopped");
    }

    public int ExecuteSteps(int limit, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(limit);
        var executed = 0;
        
        while (executed < limit && !cancellationToken.IsCancellationRequested)
        {
            var result = Machine.Step();
            
            if (result is null)
                break;

            var snapshot = Machine.CaptureSnapshot();
            
            Trace.Record(snapshot);
            _stepNumber++;
          
            if (_logger.IsEnabled(LogLevel.Debug)) 
                _logger.LogDebug("Instruction step {StepNumber}: {@Snapshot}", _stepNumber, snapshot);

            if (Machine.Faulted)
                _logger.LogWarning("Machine fault at step {StepNumber}: {Status}", _stepNumber, result.Value.Status);
            else if (result.Value.Status == ExecutionStatus.Halted) 
                _logger.LogInformation("Machine halted at step {StepNumber}", _stepNumber);

            executed++;
        }

        return executed;
    }

    private void ClearTrace()
    {
        Trace.Clear();
        _stepNumber = 0;
    }
}
