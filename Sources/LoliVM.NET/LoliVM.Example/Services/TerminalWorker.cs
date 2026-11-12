using LoliVM.NET;
using LoliVM.Assembly;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LoliVM.Example.Services;

public sealed class TerminalWorker : BackgroundService
{
    private readonly Assembler _assembler;
    private readonly Terminal _terminal;
    private readonly MicroVm _machine;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<TerminalWorker> _logger;

    public TerminalWorker(Terminal terminal,
        MicroVm machine,
        IHostApplicationLifetime lifetime,
        ILogger<TerminalWorker> logger,
        Assembler assembler)
    {
        _assembler = assembler;
        _terminal = terminal;
        _machine = machine;
        _lifetime = lifetime;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var program = _assembler.Assemble("""
                MOV R0, 1
                MOV R1, 4
                ADD R0, R1
                HALT
                """);
            if (!program.Success || !_machine.LoadProgram(program.Bytecode))
            {
                throw new InvalidOperationException("Failed to assemble or load sample firmware.");
            }
            _logger.LogInformation("Sample firmware loaded");
          
            await _terminal.RunAsync(stoppingToken);
        }
        catch (Exception exception)
        {
            _logger.LogCritical(exception, "Terminal application failed");
            Environment.ExitCode = 1;
            throw;
        }
        finally
        {
            _lifetime.StopApplication();
        }
    }
}
