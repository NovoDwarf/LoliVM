using LoliVM.Assembly;
using LoliVM.Example.Services;
using LoliVM.Example.Commands;
using LoliVM.Example.Models;
using LoliVM.NET;
using Microsoft.Extensions.DependencyInjection;

namespace LoliVM.Example.Extensions;

public static class TerminalServicesExtensions
{
    public static IServiceCollection AddVmTerminal(this IServiceCollection services)
    {
        services.AddSingleton<MicroVm>();
        services.AddSingleton<MachineSession>();
        services.AddSingleton<Assembler>();
        services.AddSingleton<ProgramEditor>();
        services.AddSingleton<Terminal>();
        
        services.AddSingleton<ITerminalCommand, HelpCommand>();
        services.AddSingleton<ITerminalCommand, StartCommand>();
        services.AddSingleton<ITerminalCommand, ResetCommand>();
        services.AddSingleton<ITerminalCommand, StopCommand>();
        services.AddSingleton<ITerminalCommand, StepCommand>();
        services.AddSingleton<ITerminalCommand, RunCommand>();
        services.AddSingleton<ITerminalCommand, StatusCommand>();
        services.AddSingleton<ITerminalCommand, DumpCommand>();
        services.AddSingleton<ITerminalCommand, EditCommand>();
        services.AddSingleton<ITerminalCommand, ExitCommand>();
        
        return services;
    }
}
