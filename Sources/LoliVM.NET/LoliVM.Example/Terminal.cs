using LoliVM.Example.Services;
using LoliVM.Example.Commands;
using LoliVM.Example.Models;
using Microsoft.Extensions.Logging;

namespace LoliVM.Example;

public sealed class Terminal
{
    private readonly ProgramEditor _editor;
    private readonly TextReader _input;
    private readonly TextWriter _output;
    private readonly MachineSession _session;
    private readonly ILogger<Terminal> _logger;
    private readonly Dictionary<string, ITerminalCommand> _commands;
    private readonly TerminalCommandContext _context;

    public Terminal(MachineSession session,
        TextReader input,
        TextWriter output,
        IEnumerable<ITerminalCommand> commands,
        ILogger<Terminal> logger,
        ProgramEditor editor)
    {
        ArgumentNullException.ThrowIfNull(editor);
        _editor = editor;
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(commands);
        ArgumentNullException.ThrowIfNull(logger);
      
        _session = session;
        _input = input;
        _output = output;
        _logger = logger;
       
        var registeredCommands = commands.ToArray();
        _commands = new Dictionary<string, ITerminalCommand>(StringComparer.OrdinalIgnoreCase);
        
        foreach (var command in registeredCommands)
        {
            if (string.IsNullOrWhiteSpace(command.Name) || command.Name.Any(char.IsWhiteSpace))
                throw new ArgumentException("Command names must be nonempty single words.", nameof(commands));

            if (!_commands.TryAdd(command.Name, command))
                throw new ArgumentException($"Duplicate terminal command: {command.Name}", nameof(commands));
        }

        _context = new TerminalCommandContext(session, output, Array.AsReadOnly(registeredCommands));
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        await _output.WriteLineAsync("LoliVM terminal. Type help for commands.", cancellationToken);
     
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await _output.WriteAsync(_editor.Active ? _editor.Prompt : "> ", cancellationToken);
                var line = await _input.ReadLineAsync(cancellationToken);
                
                if (line is null)
                {
                    _logger.LogInformation("Terminal input closed");
                    return;
                }

                if (_editor.Active)
                {
                    _editor.ProcessLine(line, _output);
                    continue;
                }

                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
               
                if (parts.Length == 0)
                    continue;

                if (!_commands.TryGetValue(parts[0], out var command))
                {
                    _logger.LogDebug("Unknown terminal command {Command}", parts[0]);
                    await _output.WriteLineAsync("Unknown command. Type help.", cancellationToken);
                    continue;
                }

                if (!command.Execute(_context, parts[1..], cancellationToken))
                    return;
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Terminal shutdown requested");
        }
        finally
        {
            _editor.Discard();
            _session.Stop();
        }
    }
}
