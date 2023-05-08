using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Managed.SandboxEngine;
using NativeEngine;
using Sandbox;
using Sandbox.Diagnostics;
using Sandbox.Engine;
using Sandbox.Tasks;
using Steamworks;

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

	public void Dispose()
	{
		if ( !AppSystem.IsValid ) return;
		AppSystem.Destroy();
	}
}
