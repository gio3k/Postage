global using static Postage.Logger;
using CommandLine;
using Postage.Core;

namespace Postage;

public static class Launcher
{
	public static void Main( string[] args )
	{
		Parser.Default.ParseArguments<CommandLineOptions>( args )
			.WithParsed( Run );
	}

	public static CommandLineOptions Options { get; private set; }
	public static GameDirectory GameDirectory { get; private set; }
	public static string LauncherDirectory { get; private set; }
	public static Preloader Preloader { get; private set; }
	public static ContextHost ContextHost { get; private set; }

	private static void Run( CommandLineOptions options )
	{
		Log.Info( "Hello from Postage! *waves*" );

		LauncherDirectory = Environment.CurrentDirectory;

		Log.Info( "Initializing game dir..." );
		GameDirectory = new GameDirectory( options.Root );

		Log.Info( "Updating environment..." );
		Environment.CurrentDirectory = GameDirectory.Root;
		Environment.SetEnvironmentVariable( "PATH",
			$"{GameDirectory.Binaries};{Environment.GetEnvironmentVariable( "PATH" )}" );

		Log.Info( "Initializing preloader..." );
		Preloader = new Preloader();

		Log.Info( "Initializing interop..." );
		Interop.InitializeEngineInterop();

		Log.Info( "Initializing context host..." );
		ContextHost = new ContextHost();
		ContextHost.Init();
	}
}

public class CommandLineOptions
{
	[Option( "root", Required = false, HelpText = "s&box root - should contain bin & core" )]
	public string Root { get; set; }

	[Option( "app", Required = false, HelpText = "Game addon directory" )]
	public string AppContent { get; set; }

	[Option( "apx", Required = false, HelpText = ".NET assembly to use as the main game assembly" )]
	public string AppAssembly { get; set; }

	[Option( "lib", Required = false, HelpText = ".NET assemblies to add" )]
	public IEnumerable<string> Libraries { get; set; } = new List<string>();

	[Option( 'v', "verbose", Required = false, HelpText = "Print all debug messages?", Default = false )]
	public bool Verbose { get; set; }
}
