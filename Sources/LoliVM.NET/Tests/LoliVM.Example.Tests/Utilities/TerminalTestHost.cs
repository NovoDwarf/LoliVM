using LoliVM.Example;
using LoliVM.Example.Extensions;
using LoliVM.NET;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace LoliVM.Example.Tests.Utilities;

internal sealed class TerminalTestHost : IDisposable
{
    private readonly ServiceProvider _provider;

    public TerminalTestHost(MicroVm machine, TextReader input, TextWriter output, ILoggerFactory? loggerFactory = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddVmTerminal();
        services.AddSingleton(machine);
        services.AddSingleton(input);
        services.AddSingleton(output);
        services.AddSingleton<ILoggerFactory>(loggerFactory ?? NullLoggerFactory.Instance);
        _provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
    }

    public Terminal Terminal => _provider.GetRequiredService<Terminal>();
    public void Dispose() => _provider.Dispose();
}
