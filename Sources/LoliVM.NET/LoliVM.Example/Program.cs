using LoliVM.Example.Extensions;
using LoliVM.Example.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Formatting.Json;

namespace LoliVM.Example;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        
        builder.Logging.ClearProviders();
        builder.Services.AddSerilog((_, configuration) => configuration
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .WriteTo.File(
                new JsonFormatter(),
                Path.Combine(AppContext.BaseDirectory, "logs", "lolivm-.jsonl"),
                rollingInterval: RollingInterval.Day,
                fileSizeLimitBytes: 10 * 1024 * 1024,
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: 7));
        
        builder.Services.AddSingleton(Console.In);
        builder.Services.AddSingleton(Console.Out);
        builder.Services.AddVmTerminal();
        builder.Services.AddHostedService<TerminalWorker>();

        using var host = builder.Build();
        await host.RunAsync();
    }
}
