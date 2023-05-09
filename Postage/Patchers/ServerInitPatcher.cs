using System.Reflection;
using HarmonyLib;
using NativeEngine;
using Sandbox;
using Sandbox.Engine;
using Sandbox.Internal;

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

	private static TypeLibrary GetTypeLibrary()
	{
		var assembly = Engine.ServerContext.Assemblies.SingleOrDefault( v => v.GetName().Name == "Sandbox.Game" );
		if ( assembly == null )
			throw new Exception( "Couldn't find Sandbox.Game in server context" );
		var type = assembly.GetType( "Sandbox.Internal.GlobalGameNamespace" );
		if ( type == null )
			throw new Exception( "Couldn't find Sandbox.Internal.GlobalGameNamespace" );
		if ( type.Property( "TypeLibrary" ).GetValue( null ) is not TypeLibrary tl )
			throw new Exception( "Unknown value from GlobalGameNamespace.TypeLibrary" );
		return tl;
	}

	private static bool Prefix( IServerDll __instance )
	{
		Log.Info( "Running Postage server init..." );

		var server = new ServerDllShim( __instance );

		// Just be normal for a little bit - do what the stock server does
		IMenuAddon.SetLoadingStatus( "Postage", "Preparing" );

		var typeLibrary = GetTypeLibrary();

		server.PackageLoader = new PackageLoader( typeLibrary, "game" );
		server.PackageLoader.OnAssemblyLoaded = ( s, bytes ) =>
		{
			Log.Info( $"todo: impl OnAssemblyLoaded! {bytes.Length}" );
		};

		if ( Game.HotloadManager == null )
			Log.Warn( "HotloadManager was null?" );
		else
			Game.HotloadManager.AssemblyResolver = server.PackageLoader.AccessControl;

		// Here starts our special code - use our app package
		Log.Info( "Finding our game package..." );
		if ( Postage.AppPackage == null )
			throw new Exception( "No app package loaded" );
		ServerContext.GamePackage = Postage.AppPackage;

		// Load the package
		Log.Info( "Loading the game package..." );
		server.PackageLoader.LoadPackage( ServerContext.GamePackage.Package.FullIdent );

		// Back to normal s&box code!
		Log.Info( $"Current gamemode: {ServerContext.GamePackage.Package.Title}" );
		PackageManager.OnPackageInstalledToContext += ( package, s ) =>
		{
			Log.Info( $"todo: impl OnPackageInstalledToContext! {package.Package.Title}, {s}" );
		};

		var package = ServerContext.GamePackage.Package;
		Game.serverInformation.GameTitle = package.Title;
		ServerConfig.ServerInit();
		ResourceLoader.LoadAllGameResource( FileSystem.Mounted, true );
		Game.ServerSteamId = EngineGlue.GetServerSteamId();

		// Try to load our GameManager
		TypeDescription managerTypeDescription = null;
		foreach ( var typeDescription in GlobalGameNamespace.TypeLibrary.GetTypes<BaseGameManager>() )
		{
			if ( typeDescription.IsAbstract ) continue;
			Log.Info( $"Found BaseGameManager: {typeDescription.Name}" );
			managerTypeDescription = typeDescription;
		}

		if ( managerTypeDescription == null )
			throw new Exception( "No GameManager found" );

		Log.Info( $"Creating a GameManager from {managerTypeDescription.FullName}" );
		BaseGameManager.Current = managerTypeDescription.Create<BaseGameManager>();

		NetworkAssetList.Initialize();
		IMenuAddon.SetLoadingStatus( "Postage", "Finalized Prefix" );

		return false;
	}
}
