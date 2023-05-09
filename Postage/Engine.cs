using System.Diagnostics;
using System.Runtime.InteropServices;
using Managed.SandboxEngine;
using NativeEngine;
using Sandbox;
using Sandbox.Internal;

namespace Postage;

public static class Engine
{
	private static CMaterialSystem2AppSystemDict AppSystem { get; set; }

	public static PostageLoadContext ClientContext { get; private set; }
	public static PostageLoadContext MenuContext { get; private set; }
	public static PostageLoadContext ServerContext { get; private set; }

	public static void Init( string root )
	{
		Log.Info( "Preloading engine2..." );
		if ( !NativeLibrary.TryLoad( $"{Postage.BinDirectory}\\engine2.dll", out var handle ) )
			throw new Exception( "Couldn't load engine2.dll" );

		Log.Info( "Updating environment..." );
		Environment.CurrentDirectory = Postage.RootDirectory;
		Environment.SetEnvironmentVariable( "PATH",
			$"{Postage.BinDirectory};{Environment.GetEnvironmentVariable( "PATH" )}" );

		Log.Info( "Initializing NativeInterop..." );
		NativeInterop.Initialize();

		Log.Info( "Initializing custom Postage Interop..." );
		Interop.Init( Postage.RootDirectory );

		// MenuContext = new PostageLoadContext( Postage.LibDirectory, "Sandbox.Menu" );
		ClientContext = new PostageLoadContext( Postage.LibDirectory, "Sandbox.Client" );
		ClientContext.AddSecondary( "Sandbox.Server" );
		ServerContext = ClientContext;
		//ServerContext = new PostageLoadContext( Postage.LibDirectory, "Sandbox.Server" );
		// new PostageLoadContext( LibDirectory, "Sandbox.Tools" );

		EngineGlobal.Plat_SetCurrentDirectory( Postage.RootDirectory );

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

	public static bool RunFrame()
	{
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
