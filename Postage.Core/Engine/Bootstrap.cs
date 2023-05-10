using System.Reflection;
using System.Runtime.CompilerServices;
using NativeEngine;
using Sandbox;
using Sandbox.Diagnostics;
using Sandbox.Engine;
using Sandbox.Internal;
using Sandbox.Tasks;

namespace Postage.Core.Engine;

public class PostageBootstrap
{
	public static bool IsDedicated { get; private set; }
	public static bool IsRetail { get; private set; }
	public static bool IsInToolsMode { get; private set; }
	public static bool IsInTestMode { get; private set; }

	public static void PreInit( string rootFolder, bool isDedicated, bool isRetail, bool isTools, bool isTest,
		int netVersion,
		int apiVersion )
	{
		IsDedicated = isDedicated;
		IsRetail = isRetail;
		IsInToolsMode = isTools;
		IsInTestMode = isTest;

		ThreadSafe.MarkMainThread();

		Protocol.Network = netVersion;
		Protocol.Api = apiVersion;

		SyncContext.Init();

		Logging.Enabled = true;
		Logging.PrintToConsole = true;

		try
		{
			Log.Info( "Initializing engine file system..." );
			FileSystemHelper.Initialize( rootFolder );
			EngineFileSystem.Root.CreateDirectory( "/cache" );
			PackageManager.PackageTemporaryDownloadFolder = EngineFileSystem.Root.GetFullPath( "/cache" );
		}
		catch ( Exception e )
		{
			Log.Info( e );
		}

		Log.Info( "Bootstrapping contexts..." );
		IClientDll.Current?.Bootstrap();
		IMenuDll.Current?.Bootstrap();
		NoMenuHelper.Bootstrap();
		IToolsDll.Current?.Bootstrap();
		IServerDll.Current?.Bootstrap();
	}

	public static void Init()
	{
		IToolsDll.Current?.Spin();

		LocalProject.Startup();

		Log.Info( "Loading static constructors for Sandbox.System..." );
		foreach ( var type in Assembly.Load( "Sandbox.System" ).GetTypes() )
			RuntimeHelpers.RunClassConstructor( type.TypeHandle );

		Log.Info( "Loading static constructors for Sandbox.Engine..." );
		foreach ( var type in Assembly.Load( "Sandbox.Engine" ).GetTypes() )
			RuntimeHelpers.RunClassConstructor( type.TypeHandle );

		Log.Info( "Post bootstrapping tools (if possible)" );
		IToolsDll.Current?.OnPostBoostrap();

		try
		{
			Log.Info( "Post bootstrapping other contexts..." );
			IServerDll.Current?.PostBootstrap();
			IMenuDll.Current?.PostBootstrap();
			NoMenuHelper.PostBootstrap();
			IClientDll.Current?.PostBootstrap();

			Log.Info( "Setting TypeLibrary.OnClassName!" );
			typeof(TypeLibrary).GetField( "OnClassName", BindingFlags.NonPublic | BindingFlags.Static )!
				.SetValue( null, new Action<string>( v => StringToken.FindOrCreate( v ) ) );

			Log.Info( "Finishing up..." );
			Bootstrap.LoadingFinished();
			Gizmo.Engine.Initialize();
		}
		catch ( Exception e )
		{
			Log.Info( e );
		}
	}
}
