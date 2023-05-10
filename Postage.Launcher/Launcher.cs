global using static Postage.Logger;
using CommandLine;
using Postage.Core;
using Postage.Core.Engine;

namespace Postage;

public static class Launcher
{
	public static void Main( string[] args )
	{
		LauncherDirectory = Environment.CurrentDirectory;
		Parser.Default.ParseArguments<CommandLineOptions>( args )
			.WithParsed( Run );
	}

	public static CommandLineOptions Options { get; private set; }
	public static GameDirectory GameDirectory { get; private set; }
	public static string LauncherDirectory { get; private set; }
	public static Source2Instance Engine;

	internal static List<(string, byte[])> Libraries { get; } = new();

	private static void AddGamePackage()
	{
		try
		{
			var assemblies = new List<string>();
			var project = new Project { AddonDirectory = Options.AppContent };

			assemblies.Add( Options.AppAssembly );

			foreach ( var library in Options.Libraries )
			{
				var split = library.Split( ';' );
				if ( split.Length != 2 )
					throw new Exception( $"Invalid library {library}" );
				Libraries.Add( (split[0], File.ReadAllBytes( split[1] )) );
				assemblies.Add( split[1] );
			}

			project.Assemblies = assemblies;

			ProjectCreator.MakeGameProject( project );
		}
		catch ( Exception e )
		{
			Log.Info( e );
		}
	}

	private static void Run( CommandLineOptions options )
	{
		Options = options;

		Log.Info( "Hello from Postage! *waves*" );

		AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolver.AssemblyResolve;

		Log.Info( "Initializing game dir..." );
		GameDirectory = new GameDirectory( options.Root );

		Log.Info( "Updating environment..." );
		Environment.CurrentDirectory = GameDirectory.Root;
		Environment.SetEnvironmentVariable( "PATH",
			$"{GameDirectory.Binaries};{Environment.GetEnvironmentVariable( "PATH" )}" );

		Log.Info( "Preparing to start the engine..." );
		Engine = new Source2Instance( GameDirectory );

		Log.Info( "Applying runtime patches before engine init..." );
		Patches.Apply();

		Log.Info( "Starting the engine!" );
		Engine.Initialize();

		Log.Info( "Adding game package..." );
		AddGamePackage();

		Log.Info( "Adding ServerModifier to the server context..." );
		try
		{
			Engine.Contexts.Server.AddMainAssembly( $"{LauncherDirectory}\\Postage.ServerModifier.dll",
				true );
		}
		catch ( Exception e )
		{
			Log.Info( e );
		}

		Log.Info( "Starting engine loop..." );
		Engine.StartLooping();
	}
}

public class CommandLineOptions
{
	[Option( "root", Required = true, HelpText = "s&box root - should contain bin & core" )]
	public string Root { get; set; }

	[Option( "app", Required = true, HelpText = "Game addon directory" )]
	public string AppContent { get; set; }

	[Option( "apx", Required = true, HelpText = ".NET assembly to use as the main game assembly" )]
	public string AppAssembly { get; set; }

	[Option( "lib", Required = false, HelpText = ".NET assemblies to add" )]
	public IEnumerable<string> Libraries { get; set; } = new List<string>();

	[Option( 'v', "verbose", Required = false, HelpText = "Print all debug messages?", Default = false )]
	public bool Verbose { get; set; }
}
