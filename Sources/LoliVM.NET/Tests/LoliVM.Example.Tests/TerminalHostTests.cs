using LoliVM.Example.Tests.Utilities;
using LoliVM.Example;
using LoliVM.Example.Commands;
using LoliVM.Example.Extensions;
using LoliVM.Example.Models;
using LoliVM.Example.Services;
using LoliVM.NET;
using LoliVM.Tests.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace LoliVM.Example.Tests;

public sealed class TerminalHostTests
{
    [Fact]
    public async Task Host_ExitsAndStopsMachineAfterTerminalExit()
    {
        // Arrange
        var output = new StringWriter();
        var builder = Host.CreateApplicationBuilder();
      
        builder.Logging.ClearProviders();
        builder.Services.AddVmTerminal();
        builder.Services.AddSingleton<TextReader>(new StringReader("start\nrun\nexit\n"));
        builder.Services.AddSingleton<TextWriter>(output);
        builder.Services.AddHostedService<TerminalWorker>();
       
        using var host = builder.Build();
        var machine = host.Services.GetRequiredService<MicroVm>();
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        
        timeout.CancelAfter(TimeSpan.FromSeconds(10));

        // Act
        await host.RunAsync(timeout.Token);

        // Assert
        Assert.False(timeout.IsCancellationRequested);
        Assert.False(machine.Running);
        Assert.Contains("Executed 4 step(s)", output.ToString());
    }

    [Fact]
    public async Task RegisteredStrategy_IsDispatchedAndIncludedInHelp()
    {
        // Arrange
        var output = new StringWriter();
        var services = new ServiceCollection();
        
        services.AddLogging();
        services.AddVmTerminal();
        services.AddSingleton<TextReader>(new StringReader("help\nPiNg\nexit\n"));
        services.AddSingleton<TextWriter>(output);
        services.AddSingleton<ITerminalCommand, PingCommand>();
        
        await using var provider = services.BuildServiceProvider();
        var terminal = provider.GetRequiredService<Terminal>();

        // Act
        await terminal.RunAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Contains(" | ping", output.ToString());
        Assert.Contains("pong", output.ToString());
    }

    [Fact]
    public void Constructor_RejectsDuplicateNamesIgnoringCase()
    {
        // Arrange
        var session = new MachineSession(new MicroVm(), NullLogger<MachineSession>.Instance);
        ITerminalCommand[] commands = [new PingCommand(), new PingCommand()];

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Terminal(session, new StringReader(""),
            new StringWriter(), commands, NullLogger<Terminal>.Instance,
            new ProgramEditor(new LoliVM.Assembly.Assembler(), session)));
    }

    [Fact]
    public async Task Cancellation_StopsMachineWhileWaitingForInput()
    {
        // Arrange
        var machine = TestPrograms.CreateMachine();
        machine.Start();
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var reader = new WaitingReader();
        using var application = new TerminalTestHost(machine, reader, new StringWriter());

        // Act
        var run = application.Terminal.RunAsync(cancellation.Token);
        await cancellation.CancelAsync();
        await run;

        // Assert
        Assert.False(machine.Running);
    }

    private sealed class PingCommand : ITerminalCommand
    {
        public string Name => "ping";
        public string Usage => "ping";

        public bool Execute(TerminalCommandContext context, string[] arguments, CancellationToken cancellationToken)
        {
            context.Output.WriteLine("pong");
            return true;
        }
    }

    private sealed class WaitingReader : TextReader
    {
        public override async ValueTask<string?> ReadLineAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return null;
        }
    }
}
