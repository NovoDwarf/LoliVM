using LoliVM.Example.Tests.Utilities;
using LoliVM.NET.Enums;
using LoliVM.NET;
using LoliVM.Tests.Shared;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Xunit;

namespace LoliVM.Example.Tests;

public sealed class TerminalLoggingTests
{
    [Fact]
    public async Task Run_LogsStructuredStepsAndLifecycleThroughSerilog()
    {
        // Arrange
        var sink = new CollectingSink();
        
        await using var logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.Sink(sink).CreateLogger();
       
        using var factory = LoggerFactory.Create(builder => builder.AddSerilog(logger));
        var machine = TestPrograms.CreateMachine();
        
        using var application = new TerminalTestHost(machine, new StringReader("start\nrun\nexit\n"), new StringWriter(), factory);

        // Act
        await application.Terminal.RunAsync(TestContext.Current.CancellationToken);
        var steps = sink.Events.Where(entry => entry.Properties.ContainsKey("Snapshot")).ToArray();

        // Assert
        Assert.Equal(4, steps.Length);
        Assert.Equal(new ScalarValue(4L), steps[^1].Properties["StepNumber"]);
        
        var snapshot = Assert.IsType<StructureValue>(steps[^1].Properties["Snapshot"]);
        
        Assert.Contains(snapshot.Properties, property => property.Name == "Halted" && property.Value.Equals(new ScalarValue(true)));
        Assert.Contains(sink.Events, entry => entry.MessageTemplate.Text == "Machine started");
        Assert.Contains(sink.Events, entry => entry.MessageTemplate.Text == "Machine halted at step {StepNumber}");
        Assert.Contains(sink.Events, entry => entry.MessageTemplate.Text == "Machine stopped");
    }

    [Fact]
    public async Task Run_LogsGuestFaultAsWarning()
    {
        // Arrange
        var sink = new CollectingSink();
       
        await using var logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.Sink(sink).CreateLogger();
        using var factory = LoggerFactory.Create(builder => builder.AddSerilog(logger));
      
        var machine = new MicroVm();
        machine.LoadProgram([0xFE]);
       
        using var application = new TerminalTestHost(machine, new StringReader("start\nrun\nexit\n"),
            new StringWriter(), factory);

        // Act
        await application.Terminal.RunAsync(TestContext.Current.CancellationToken);

        // Assert
        var fault = Assert.Single(sink.Events, entry => entry.Level == LogEventLevel.Warning);
        Assert.Equal(new ScalarValue(ExecutionStatus.InvalidInstruction), fault.Properties["Status"]);
        Assert.True(machine.Faulted);
        Assert.DoesNotContain(sink.Events, entry => entry.Exception is not null);
    }

    private sealed class CollectingSink : ILogEventSink
    {
        public List<LogEvent> Events { get; } = [];

        public void Emit(LogEvent logEvent)
        {
            Events.Add(logEvent);
        }
    }
}
