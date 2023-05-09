﻿global using static Postage.Logger;
using System.Reflection;
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
		var name = $"{Engine.LibDirectory}\\{trim}.dll";
		name = name.Replace( ".resources.dll", ".dll" );

		if ( File.Exists( name ) )
			return Assembly.LoadFrom( name );

		Log.Warn( $"{name} not found" );
		return null;
	}

	private Launcher( IEnumerable<string> args )
	{
		Parser.Default.ParseArguments<Options>( args )
			.WithParsed( Run )
			.WithNotParsed( v =>
			{
				Log.Info( "Launching with basic defaults for debugging" );
				var options = new Options();
				options.Root = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\sbox";
				options.Addon = "C:\\src\\fishmoba\\.addon";
				options.Verbose = true;
				Run( options );
			} )
			;
	}

	private void Run( Options options )
	{
		AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;

		RootDirectory = options.Root;

		Log.DebugEnabled = options.Verbose;

		AccessPatcher.Patch();

		Engine.Init( options.Root );
		Engine.Loop();
	}
}