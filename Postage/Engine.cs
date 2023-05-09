using System.Runtime.InteropServices;
using Managed.SandboxEngine;
using NativeEngine;
using Sandbox;
using Sandbox.Internal;

namespace Postage;

public static class Engine
{
	private static CMaterialSystem2AppSystemDict AppSystem { get; set; }

	public static string RootDirectory { get; private set; }
	public static string LibDirectory { get; private set; }
	public static string BinDirectory { get; private set; }

	public static PostageLoadContext ClientContext { get; private set; }
	public static PostageLoadContext MenuContext { get; private set; }
	public static PostageLoadContext ServerContext { get; private set; }

	public static void Init( string root )
	{
		RootDirectory = root;
		LibDirectory = root + "\\bin\\managed";
		BinDirectory = root + "\\bin\\win64";

		Log.Info( $"RootDirectory: {RootDirectory}" );
		Log.Info( $"LibDirectory: {LibDirectory}" );
		Log.Info( $"BinDirectory: {BinDirectory}" );

		Log.Info( "Preloading engine2..." );
		if ( !NativeLibrary.TryLoad( $"{BinDirectory}\\engine2.dll", out var handle ) )
			throw new Exception( "Couldn't load engine2.dll" );

		Log.Info( "Updating environment..." );
		Environment.CurrentDirectory = RootDirectory;
		Environment.SetEnvironmentVariable( "PATH", $"{BinDirectory};{Environment.GetEnvironmentVariable( "PATH" )}" );

		Log.Info( "Initializing NativeInterop..." );
		NativeInterop.Initialize();

		Log.Info( "Initializing custom Postage Interop..." );
		Interop.Init( RootDirectory );

		MenuContext = new PostageLoadContext( LibDirectory, "Sandbox.Menu" );
		ClientContext = new PostageLoadContext( LibDirectory, "Sandbox.Client" );
		ServerContext = new PostageLoadContext( LibDirectory, "Sandbox.Server" );
		// new PostageLoadContext( LibDirectory, "Sandbox.Tools" );

		EngineGlobal.Plat_SetCurrentDirectory( RootDirectory );

		// Pre-initialize engine
		Log.Info( "Initializing Source 2..." );
		if ( !EngineGlobal.SourceEnginePreInit( Environment.CommandLine.Replace( ".dll", ".exe" ), true, false ) )
			throw new Exception( "Failed to pre init Source 2 - SourceEnginePreInit fail" );

		// Initialize app
		Log.Info( "Pre-initialized engine, creating app..." );
		AppSystem = CMaterialSystem2AppSystemDict.Create( "Postage", false, true, false );

		// Initialize engine
		if ( !EngineGlobal.SourceEngineInit( AppSystem ) )
			throw new Exception( "Failed to init Source 2 - SourceEngineInit fail" );
	}

	private static bool _wait;

	public static bool RunFrame()
	{
		if ( !_wait )
		{
			if ( IMenuAddon.Current != null )
			{
				Log.Info( $"MenuAddon == {IMenuAddon.Current}" );
				IMenuAddon.Current.Init();
				_wait = true;
			}
		}

		EngineLoop.RunFrame( AppSystem, out var wantsToQuit );
		return !wantsToQuit;
	}

	public static void Loop()
	{
		EngineGlobal.Plat_SetCurrentFrame( 0uL );
		while ( RunFrame() )
		{
			BlockingLoopPumper.Run( delegate
			{
				RunFrame();
			} );
		}
	}
}
