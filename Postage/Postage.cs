global using static Postage.Logger;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using CommandLine;
using CommandLine.Text;
using NativeEngine;
using Sandbox;
using Sandbox.Diagnostics;
using Console = System.Console;

namespace Postage;

public class Postage
{
	public static void Main( string[] args ) => new Postage( args );

	public static string RootDirectory { get; private set; }
	public static string LibDirectory { get; private set; }
	public static string BinDirectory { get; private set; }

	internal static PackageManager.ActivePackage AppPackage { get; private set; }

	public static List<(string, byte[])> Libraries { get; } = new();

	public static Options LaunchOptions { get; private set; }


	public class Options
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

		[Option( "vv", Required = false, HelpText = "Print all trace messages?", Default = false )]
		public bool SuperVerbose { get; set; }
	}

	private static Assembly AssemblyResolve( object sender, ResolveEventArgs args )
	{
		var name = args.Name.Split( ',' )[0].Replace( ".resources", "" );
		Log.Info( name );

		var path = $"{LibDirectory}\\{name}.dll";

		if ( File.Exists( path ) )
			return Assembly.LoadFrom( path );

		foreach ( var (lib, bytes) in Libraries )
		{
			Log.Info( $"compare {lib}, {name}" );
			if ( lib != name && name != $"package.{lib}" )
				continue;

			Log.Info( $"Loading game library {lib}" );
			return Assembly.Load( bytes );
		}

		Log.Warn( $"{name} / {path} not found" );
		return null;
	}

	private Postage( IEnumerable<string> args )
	{
		Console.ResetColor();

		Parser.Default.ParseArguments<Options>( args )
			.WithParsed( Run );
	}

	private void Run( Options options )
	{
		LaunchOptions = options;

		AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;

		RootDirectory = options.Root;
		LibDirectory = options.Root + "\\bin\\managed";
		BinDirectory = options.Root + "\\bin\\win64";

		Log.Info( $"RootDirectory: {RootDirectory}" );
		Log.Info( $"LibDirectory: {LibDirectory}" );
		Log.Info( $"BinDirectory: {BinDirectory}" );

		Log.DebugEnabled = options.Verbose;

		if ( LaunchOptions.SuperVerbose )
			Logging.SetRule( "*", LogLevel.Trace );

		Engine.Init( options.Root );

		AccessPatcher.Patch();

		LocalProject.Initialize();

		try
		{
			foreach ( var library in options.Libraries )
			{
				var split = library.Split( ';' );
				if ( split.Length != 2 )
					throw new Exception( $"Invalid library {library}" );
				Libraries.Add( (split[0], File.ReadAllBytes( split[1] )) );
			}
		}
		catch ( Exception e )
		{
			Log.Info( e );
		}

		AppPackage = ProjectCtl.Load( new[] { options.AppAssembly }, options.AppContent );

		g_pInputService.InsertCommand( InputCommandSource.ICS_SERVER, "gamemode parparpar.razorplatformer\n", 0, 0 );
		g_pInputService.InsertCommand( InputCommandSource.ICS_SERVER, "map calico_dev\n", 0, 0 );

		Engine.Loop();
	}
}
