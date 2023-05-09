using System.Reflection;
using System.Runtime.Loader;
using HarmonyLib;
using NativeEngine;
using Sandbox;
using Sandbox.Engine;
using Sandbox.Internal;
using Sandbox.Tasks;

namespace Postage;

public static class ServerInitPatcher
{
	public static void Patch()
	{
		PatchCore.Update();
		Log.Info( "About to patch server initialization..." );

		if ( Engine.ServerContext == null )
			throw new Exception( "Can't patch - server context not ready" );

		var assembly = Engine.ServerContext.Assemblies.SingleOrDefault( v => v.GetName().Name == "Sandbox.Server" );
		if ( assembly == null )
			throw new Exception( "Couldn't load server assembly" );

		var type = assembly.GetType( "Sandbox.ServerDll" );
		if ( type == null )
			throw new Exception( "Couldn't find Sandbox.ServerDll" );

		var original = type.GetMethod( "OnServerInitialize" );

		Log.Info( "Patching OnServerInitialize..." );
		PatchCore.Harmony.Patch( original, new HarmonyMethod( Prefix ) );
	}

	private static Assembly GetAssembly( string name )
	{
		var assembly = Engine.ServerContext.Assemblies.SingleOrDefault( v => v.GetName().Name == name );
		if ( assembly == null )
			throw new Exception( $"Couldn't find {name} in server context" );
		return assembly;
	}

	private static TypeLibrary GetTypeLibrary()
	{
		var assembly = GetAssembly( "Sandbox.Game" );
		var type = assembly.GetType( "Sandbox.Internal.GlobalGameNamespace" );
		if ( type == null )
			throw new Exception( "Couldn't find Sandbox.Internal.GlobalGameNamespace" );
		if ( type.Property( "TypeLibrary" ).GetValue( null ) is not TypeLibrary tl )
			throw new Exception( "Unknown value from GlobalGameNamespace.TypeLibrary" );
		return tl;
	}

	private static HotloadManager GetHotloadManager()
	{
		var assembly = GetAssembly( "Sandbox.Game" );
		var type = assembly.GetType( "Sandbox.Game" );
		if ( type == null )
			throw new Exception( "Couldn't find Sandbox.Game" );
		if ( type.Property( "HotloadManager" ).GetValue( null ) is not HotloadManager v )
			throw new Exception( "Unknown value from HotloadManager" );
		return v;
	}

	private static bool Prefix( IServerDll __instance )
	{
		Log.Info( "Running Postage server init..." );

		var server = new ServerDllShim( __instance );

		// Just be normal for a little bit - do what the stock server does
		IMenuAddon.SetLoadingStatus( "Postage", "Preparing" );

		// Because of context weirdness, we need to use reflection to do most of this stuff
		var typeLibrary = GetTypeLibrary();
		var hotloadManager = GetHotloadManager();

		server.PackageLoader = new PackageLoader( typeLibrary, "game" );
		server.PackageLoader.OnAssemblyLoaded = ( s, bytes ) =>
		{
			server.OnPackageAssemblyLoaded( s, bytes );

			// Try to load our GameManager
			Type managerType = null;

			foreach ( var (key, value) in typeLibrary.loadedAssemblies )
			{
				foreach ( var type in value.GetTypes() )
				{
					if ( type.IsAbstract ) continue;
					if ( type.BaseType?.Name != "GameManager" )
						continue;

					managerType = type;
					Log.Info( $"Using GameManager {type.Name}" );
				}
			}

			if ( managerType == null )
			{
				Log.Info( "No GameManager found, waiting for next assembly..." );
				return;
			}

			Log.Info( $"Creating a GameManager from {managerType.FullName}" );

			GlobalGameNamespace.TypeLibrary = typeLibrary;

			{
				// BaseGameManager.Current
				var type = GetAssembly( "Sandbox.Game" ).GetType( "Sandbox.BaseGameManager" );
				var property = type.Property( "Current" );
				try
				{
					property.SetValue( null, Activator.CreateInstance( managerType ) );
				}
				catch ( Exception e )
				{
					Log.Warn( $"Failed to create GameManager instance: {e}" );
				}

				Log.Info( "Set BaseGameManager.Current" );
			}

			{
				// NetworkAssetList.Initialize()
				var type = GetAssembly( "Sandbox.Game" ).GetType( "Sandbox.NetworkAssetList" );
				var method = type.Method( "Initialize" );
				method.Invoke( null, null );
				Log.Info( "Called NetworkAssetList.Initialize()" );
			}

			IMenuAddon.SetLoadingStatus( "Postage", "Finalized Prefix" );
		};

		// Here starts our special code - use our app package
		Log.Info( "Finding our game package..." );
		if ( Postage.AppPackage == null )
			throw new Exception( "No app package loaded" );
		ServerContext.GamePackage = Postage.AppPackage;

		// Load the package
		Log.Info( "Loading the game package..." );
		server.PackageLoader.LoadPackage( ServerContext.GamePackage.Package.FullIdent );

		if ( hotloadManager == null )
			Log.Warn( "HotloadManager was null?" );
		else
			hotloadManager.AssemblyResolver = server.PackageLoader.AccessControl;

		// Back to normal s&box code!
		Log.Info( $"Current gamemode: {ServerContext.GamePackage.Package.Title}" );
		PackageManager.OnPackageInstalledToContext += ( package, s ) =>
		{
			Log.Info( $"OnPackageInstalledToContext! {package.Package.Title}, {s}" );
			server.OnServerPackageInstalled( package, s );
		};

		// Now begins reflection hell
		var package = ServerContext.GamePackage.Package;

		{
			// Game.serverInformation.GameTitle
			var type = GetAssembly( "Sandbox.Game" ).GetType( "Sandbox.Game" );
			var field = type.Field( "serverInformation" );
			var value = (ServerInformation)field.GetValue( null );
			value.GameTitle = package.Title;
			field.SetValue( null, value );
			Log.Info( "Set Game.serverInformation.GameTitle" );
		}

		{
			// ServerConfig.ServerInit()
			var type = GetAssembly( "Sandbox.Game" ).GetType( "Sandbox.ServerConfig" );
			var method = type.Method( "ServerInit" );
			method.Invoke( null, null );
			Log.Info( "Called ServerConfig.ServerInit()" );
		}

		{
			// ResourceLoader.LoadAllGameResource( FileSystem.Mounted, true )
			var typeA = GetAssembly( "Sandbox.Game" ).GetType( "Sandbox.ResourceLoader" );
			var typeB = GetAssembly( "Sandbox.Game" ).GetType( "Sandbox.FileSystem" );

			// FileSystem.Mounted
			var propertyB = typeB.Property( "Mounted" );
			var valueB = propertyB.GetValue( null );

			// ResourceLoader.LoadAllGameResource
			var methodA = typeA.Method( "LoadAllGameResource" );
			methodA.Invoke( null, new object[] { valueB, true } );
			Log.Info( "Called ResourceLoader.LoadAllGameResource()" );
		}

		{
			// Game.ServerSteamId
			var type = GetAssembly( "Sandbox.Game" ).GetType( "Sandbox.Game" );
			var property = type.Property( "ServerSteamId" );
			property.SetValue( null, EngineGlue.GetServerSteamId() );
			Log.Info( "Set Game.ServerSteamId" );
		}

		return false;
	}
}
