using VPKTools.Pak.Shared;
using VPKTools.Tier0.Shared.CommandLine;
using VPKTools.Tier0.Shared.Interfaces;
using VPKTools.Tier0.Shared.Terminal;

public class Application
{
    public static async Task<int> Main(string[] args)
    {
        var previousEncoding = Console.OutputEncoding;
        ConsoleEncoding.EnsureUtf8();

        try
        {
            return await RunAsync(args).ConfigureAwait(false);
        }
        finally
        {
            ConsoleEncoding.Restore(previousEncoding);
        }
    }

    private static async Task<int> RunAsync(string[] args)
    {
        InterfaceSystem.LoadModule("VPKTools.Tier0");
        InterfaceSystem.LoadModule("VPKTools.Pak");

        var cli = InterfaceSystem.GetInterface<ICommandLine>(InterfaceNames.CommandLine)!;
        cli.Initialize(args);

        var pak = InterfaceSystem.GetInterface<IPakSystem>(PakInterfaceNames.Pak)!;

        if (PakCliHandler.TryRun(cli, pak, out int exitCode))
        {
            return exitCode;
        }

        return RunTerminal();
    }

    private static int RunTerminal()
    {
        var terminal = InterfaceSystem.GetInterface<ITerminal>(InterfaceNames.Terminal);

        if (terminal == null)
        {
            Console.Error.WriteLine("The terminal is unavailable. Run with -help for command-line usage.");
            return 1;
        }

        terminal.Run();
        return 0;
    }
}
