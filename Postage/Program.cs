global using static Postage.Logger;
using System.Reflection;
using System.Runtime.InteropServices;
using CommandLine;

namespace Postage;

public class Launcher
{
	public static void Main( string[] args ) => new Launcher( args );

	public static string RootDirectory { get; private set; }

	private class Options
	{
		[Option( "root", Required = true, HelpText = "s&box root - should contain bin & core" )]
		public string Root { get; set; }

		[Option( "app", Required = true, HelpText = "s&box game addon to run" )]
		public string Addon { get; set; }

		[Option( 'v', "verbose", Required = false, HelpText = "print all debug messages?", Default = false )]
		public bool Verbose { get; set; }
	}

	private static Assembly AssemblyResolve( object sender, ResolveEventArgs args )
	{
		var trim = args.Name.Split( ',' )[0];
		var name = $"{RootDirectory}\\bin\\managed\\{trim}.dll";

		return File.Exists( name ) ? Assembly.LoadFrom( name ) : null;
	}

	private Launcher( IEnumerable<string> args )
	{
		AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
		Parser.Default.ParseArguments<Options>( args )
			.WithParsed( Run );
	}

	private void Run( Options options )
	{
		RootDirectory = options.Root;

		Log.DebugEnabled = options.Verbose;

		try
		{
			var app = new EngineApp( options.Root );
			app.LoadApp( options.Addon );
			app.Start();
		}
		catch ( Exception e )
		{
			Log.Error( e );
		}
	}
}
