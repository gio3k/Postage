using System.Runtime.InteropServices;
using NativeEngine;

namespace Postage;

public class EngineApp : IDisposable
{
	internal CMaterialSystem2AppSystemDict AppSystem { get; set; }

	private string RootDirectory { get; }
	private string LibDirectory { get; }
	private string BinDirectory { get; }

	public EngineApp( string root )
	{
		RootDirectory = root;
		LibDirectory = root + "\\bin\\managed";
		BinDirectory = root + "\\bin\\win64";

		Log.Info( $"RootDirectory: {RootDirectory}" );
		Log.Info( $"LibDirectory: {LibDirectory}" );
		Log.Info( $"BinDirectory: {BinDirectory}" );

		Log.Info( "Updating environment..." );
		Environment.CurrentDirectory = RootDirectory;
		Environment.SetEnvironmentVariable( "PATH", $"{BinDirectory};{Environment.GetEnvironmentVariable( "PATH" )}" );

		// Preload engine2 before interop
		Log.Info( "Preloading engine2..." );
		NativeLibrary.Load( $"{BinDirectory}\\engine2.dll" );

		Log.Info( "Initializing interop..." );
		Interop.Init( RootDirectory );

		EngineGlobal.Plat_SetCurrentDirectory( RootDirectory );

		// Initialize engine
		Log.Info( "Initializing Source 2..." );
		if ( !EngineGlobal.SourceEnginePreInit( Environment.CommandLine.Replace( ".dll", ".exe" ), true, false ) )
			throw new Exception( "Failed to init Source 2 - SourceEnginePreInit fail" );

		// Initialize app
		Log.Info( "Initialized engine, initializing app..." );
		AppSystem = CMaterialSystem2AppSystemDict.Create( "Postage", false, false, false );
		AppSystem.SuppressCOMInitialization();
		AppSystem.SuppressStartupManifestLoad( b: true );
		AppSystem.SetModGameSubdir( "core" );
		AppSystem.Init();
		AppSystem.AddSystem( "scenesystem", "SceneSystem_002" );
		
		
	}

	public void Dispose()
	{
		if ( !AppSystem.IsValid ) return;
		AppSystem.Destroy();
	}
}
