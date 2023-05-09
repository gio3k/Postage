using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NativeEngine;
using Sandbox;
using Sandbox.Engine;
using Sandbox.Internal;
using Sandbox.Tasks;
using Steamworks;

namespace Postage;

public static class Hooks
{
	[UnmanagedCallersOnly]
	public static int SandboxEngine_Bootstrap_PreInit( nint ptrRoot, int iDedicated, int iRetail,
		int iTools, int iTest, int iNet, int iApi )
	{
		Log.Info( "PreInit hook worked!" );

		var root = Sandbox.Interop.GetString( ptrRoot );
		var isDedicated = iDedicated == 1;
		var isRetail = iRetail == 1;
		var isTools = iTools == 1;
		var isTest = iTest == 1;

		Log.Info( $"dedicated {isDedicated}, retail {isRetail}, tools {isTools}, test {isTest}" );

		Log.Debug( "ThreadSafe.MarkMainThread()" );
		ThreadSafe.MarkMainThread();

		Log.Info( "Updating networking..." );
		Protocol.Network = iNet;
		Protocol.Api = iApi;

		Log.Info( "Populating engine bootstrap..." );
		typeof(Bootstrap).GetProperty( "IsDedicatedServer", BindingFlags.NonPublic | BindingFlags.Static )
			.SetValue( null, isDedicated );
		typeof(Bootstrap).GetProperty( "IsToolsMode", BindingFlags.NonPublic | BindingFlags.Static )
			.SetValue( null, isTools );
		typeof(Bootstrap).GetProperty( "IsRetail", BindingFlags.NonPublic | BindingFlags.Static )
			.SetValue( null, isRetail );

		Log.Debug( "SyncContext.Init()" );
		SyncContext.Init();

		Log.Info( "Starting filesystem init..." );
		EngineFileSystem.Initialize( root );
		EngineFileSystem.InitializeConfigFolder();
		EngineFileSystem.InitializeAddonsFolder();
		EngineFileSystem.InitializeDownloadsFolder();
		EngineFileSystem.InitializeDataFolder();
		EngineFileSystem.DownloadedFiles.CreateDirectory( "/assets" );
		PackageManager.PackageTemporaryDownloadFolder = EngineFileSystem.DownloadedFiles.GetFullPath( "/assets" );

		Log.Info( "Starting DLL bootstrapping ->" );
		if ( IClientDll.Current != null )
		{
			IClientDll.Current.Bootstrap();
			Log.Info( "bootstrapped ClientDll!" );
		}

		if ( IMenuDll.Current != null )
		{
			IMenuDll.Current.Bootstrap();
			Log.Info( "bootstrapped MenuDll!" );
		}
		else
		{
			Log.Info( "MenuDll not loaded, loading dependencies..." );
			SteamClient.Init( 590830 );
		}

		if ( IToolsDll.Current != null )
		{
			IToolsDll.Current.Bootstrap();
			Log.Info( "bootstrapped ToolsDll!" );
		}

		if ( IServerDll.Current != null )
		{
			IServerDll.Current.Bootstrap();
			Log.Info( "bootstrapped ServerDll!" );
		}

		return 1;
	}

	[UnmanagedCallersOnly]
	public static int SandboxEngine_Bootstrap_Init()
	{
		Log.Info( "Init hook worked!" );

		IToolsDll.Current?.Spin();
		IToolsDll.Current?.SetSplashStatus( "hiii" );

		Log.Info( "Loading addons..." );
		LocalProject.AddFromFileBuiltIn( "addons/base/.addon" );
		LocalProject.AddFromFileBuiltIn( "addons/menu/.addon" );
		LocalProject.Initialize();

		Log.Info( "Loading static constructors for Sandbox.System..." );
		foreach ( var type in Assembly.Load( "Sandbox.System" ).GetTypes() )
			RuntimeHelpers.RunClassConstructor( type.TypeHandle );

		Log.Info( "Loading static constructors for Sandbox.Engine..." );
		foreach ( var type in Assembly.Load( "Sandbox.Engine" ).GetTypes() )
			RuntimeHelpers.RunClassConstructor( type.TypeHandle );

		Log.Info( "Starting DLL post bootstrapping ->" );
		try
		{
			if ( IMenuDll.Current != null )
			{
				IMenuDll.Current.PostBootstrap();
				Log.Info( "post bootstrapped MenuDll!" );
			}
			else
			{
			}

			if ( IClientDll.Current != null )
			{
				IClientDll.Current.PostBootstrap();
				Log.Info( "post bootstrapped ClientDll!" );
			}

			if ( IToolsDll.Current != null )
			{
				IToolsDll.Current.OnPostBoostrap();
				Log.Info( "post bootstrapped ToolsDll!" );
			}

			if ( IServerDll.Current != null )
			{
				IServerDll.Current.PostBootstrap();
				Log.Info( "post bootstrapped ServerDll!" );
			}

			Gizmo.Engine.Initialize();

			Log.Info( "Setting TypeLibrary.OnClassName" );
			typeof(TypeLibrary).GetField( "OnClassName", BindingFlags.NonPublic | BindingFlags.Static )!
				.SetValue( null, new Action<string>( v =>
				{
					Log.Info( v );
					StringToken.FindOrCreate( v );
				} ) );

			Log.Info( "Completed init!" );
		}
		catch ( Exception e )
		{
			Log.Info( e );
		}

		return 1;
	}
}
