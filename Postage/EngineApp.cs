using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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

		Log.Info( "Updating environment..." );
		Environment.CurrentDirectory = RootDirectory;
		Environment.SetEnvironmentVariable( "PATH", $"{BinDirectory};{Environment.GetEnvironmentVariable( "PATH" )}" );

		// Preload engine2 before interop
		Log.Info( "Preloading engine2..." );
		NativeLibrary.Load( $"{BinDirectory}\\engine2.dll" );
		Log.Info( "Preloading client..." );
		NativeLibrary.Load( $"{BinDirectory}\\client.dll" );
		

		Log.Info( "Initializing interop..." );
		Interop.Init( RootDirectory );

		EngineGlobal.Plat_SetCurrentDirectory( RootDirectory );

		// Initialize engine
		Log.Info( "Initializing Source 2..." );
		if ( !EngineGlobal.SourceEnginePreInit( Environment.CommandLine.Replace( ".dll", ".exe" ), true, false ) )
			throw new Exception( "Failed to pre init Source 2 - SourceEnginePreInit fail" );

		// Initialize app
		Log.Info( "Pre-initialized engine, creating app..." );
		AppSystem = CMaterialSystem2AppSystemDict.Create( "Postage", false, true, false );
		AppSystem.SuppressCOMInitialization();
		//AppSystem.SuppressStartupManifestLoad( b: true );
		AppSystem.SetModGameSubdir( "core" );
		AppSystem.Init();
		AppSystem.AddSystem( "scenesystem", "SceneSystem_002" );

		Log.Info( "Initializing ClientDll..." );
		Assembly.Load( "Sandbox.Client" ).GetType( "AssemblyInitialize" )
			.GetMethod( "Initialize", BindingFlags.Static | BindingFlags.NonPublic ).Invoke( null, null );

		Log.Info( "Initializing ServerDll..." );
		Assembly.Load( "Sandbox.Server" ).GetType( "AssemblyInitialize" )
			.GetMethod( "Initialize", BindingFlags.Static | BindingFlags.NonPublic ).Invoke( null, null );

		Log.Info( "Proceeding to app pre-init..." );
		PostagePreInit();
	}

	private void PostagePreInit()
	{
		Log.Debug( "ThreadSafe.MarkMainThread()" );
		ThreadSafe.MarkMainThread();

		Log.Debug( "SyncContext.Init()" );
		SyncContext.Init();

		Logging.Enabled = true;
		Logging.PrintToConsole = true;

		Log.Info( "Initializing engine filesystem..." );
		EngineFileSystem.Initialize( RootDirectory );

		if ( IClientDll.Current != null )
		{
			Log.Info( "Initializing client..." );
			IClientDll.Current.Bootstrap();
		}

		if ( IServerDll.Current != null )
		{
			Log.Info( "Initializing server..." );
			IServerDll.Current.Bootstrap();
		}

		Log.Info( "Adding base project!" );
		LocalProject.AddFromFileBuiltIn( "addons/base/.addon" );

		Log.Info( "Initializing projects..." );
		PackageManager.PackageTemporaryDownloadFolder = Path.GetFullPath( "cache" );
		LocalProject.Initialize();

		Log.Info( "Loading static constructors for Sandbox.System..." );
		foreach ( var type in Assembly.Load( "Sandbox.System" ).GetTypes() )
			RuntimeHelpers.RunClassConstructor( type.TypeHandle );

		Log.Info( "Loading static constructors for Sandbox.Engine..." );
		foreach ( var type in Assembly.Load( "Sandbox.Engine" ).GetTypes() )
			RuntimeHelpers.RunClassConstructor( type.TypeHandle );

		Log.Info( "Finished pre-init!" );
	}

	/// <summary>
	/// Load app / addon from .addon file
	/// </summary>
	/// <param name="path">Path to .addon file</param>
	public void LoadApp( string path )
	{
		Log.Info( $"Adding app @ {path}" );
		LocalProject.AddFromFileBuiltIn( path );

		Log.Info( "Reinitializing projects..." );
		LocalProject.Initialize();
	}

	private bool Run()
	{
		EngineLoop.RunFrame( AppSystem, out var wantsToQuit );
		return wantsToQuit;
	}

	public void Start()
	{
		EngineGlobal.Plat_SetCurrentFrame( 0 );
		while ( Run() )
		{
		}
	}

	public void Dispose()
	{
		if ( !AppSystem.IsValid ) return;
		AppSystem.Destroy();
	}
}
