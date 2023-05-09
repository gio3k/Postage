global using static Postage.Logger;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using CommandLine;
using CommandLine.Text;
using Sandbox;
using Sandbox.Diagnostics;

namespace Postage;

public class Postage
{
	public static void Main( string[] args ) => new Postage( args );

	public static string RootDirectory { get; private set; }
	public static string LibDirectory { get; private set; }
	public static string BinDirectory { get; private set; }

	internal static PackageManager.ActivePackage AppPackage { get; private set; }

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
		var trim = args.Name.Split( ',' )[0];
		var name = $"{LibDirectory}\\{trim}.dll";
		name = name.Replace( ".resources.dll", ".dll" );

		if ( File.Exists( name ) )
			return Assembly.LoadFrom( name );

		Log.Warn( $"{name} not found" );
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

		ServerInitPatcher.Patch();
		AccessPatcher.Patch();

		LocalProject.AddFromFileBuiltIn( "addons/base/.addon" );
		LocalProject.AddFromFileBuiltIn( "addons/menu/.addon" );

		LocalProject.Initialize();
		
		AppPackage = ProjectCtl.Load( options.Libraries.Concat( new[] { options.AppAssembly } ), options.AppContent );

		Engine.Loop();
	}
}
