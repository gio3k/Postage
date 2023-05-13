using Managed.SandboxEngine;
using NativeEngine;
using Postage.Core.Patchers;
using Sandbox;
using Sandbox.Internal;

namespace Postage.Core.Engine;

public class Source2Instance
{
	internal CMaterialSystem2AppSystemDict AppSystem { get; private set; }

	public GameDirectory Directory { get; }
	public ContextHost Contexts { get; }
	public Preloader Preloader { get; }

	public Source2Instance( GameDirectory directory )
	{
		Directory = directory;

		// Initialize preloader
		Log.Info( "Initializing preloader..." );
		Preloader = new Preloader( this );

		// Initialize contexts
		Log.Info( "Initializing context host..." );
		Contexts = new ContextHost( this );

		// Initialize interop
		Log.Info( "Initializing native interop..." );
		NativeInterop.Initialize();
	}

	public void Initialize()
	{
		// Start initializing the engine!
		Log.Info( "Initializing Source 2..." );
		if ( !EngineGlobal.SourceEnginePreInit( Environment.CommandLine.Replace( ".dll", ".exe" ), true, false ) )
			throw new Exception( "Failed to pre init Source 2 - SourceEnginePreInit fail" );

		// Initialize app
		Log.Info( "Pre-initialized engine, creating app..." );
		AppSystem = CMaterialSystem2AppSystemDict.Create( "Postage", false, true, false );

		// Initialize engine
		if ( !EngineGlobal.SourceEngineInit( AppSystem ) )
			throw new Exception( "Failed to init Source 2 - SourceEngineInit fail" );

		AppSystem.SetAppWindowTitle( "Postage" );
	}

	private bool RunFrame()
	{
		EngineLoop.RunFrame( AppSystem, out var wantsToQuit );
		return !wantsToQuit;
	}

	public void StartLooping()
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
