using Postage.Core;
using Sandbox;
using Sandbox.Engine;
using Sandbox.Internal;

namespace Postage;

public static class ServerInitUtil
{
	public static object PreparePackageLoader( object instance, TypeLibrary typeLibrary )
	{
		if ( instance is not IServerDll )
			throw new Exception( "Unknown instance passed to PreparePackageLoader - needs to be ServerDll" );
		var loader = new PackageLoader( typeLibrary, "game" );

		instance.GetType().Field( "PackageLoader" ).SetValue( instance, loader );
		Log.Info( "Set instance PackageLoader!" );

		return loader;
	}

	public static AccessControl GetPackageLoaderAccessControl( object instance )
	{
		if ( instance is not PackageLoader pl )
			throw new Exception(
				"Unknown instance passed to GetPackageLoaderAccessControl - needs to be PackageLoader" );

		return pl.AccessControl;
	}

	public static void SetGamePackage() => ServerContext.GamePackage = StaticData.GamePackage;

	public static void LoadAllPackageAssemblies( object instance )
	{
		if ( instance is not PackageLoader pl )
			throw new Exception(
				"Unknown instance passed to LoadAllPackageAssemblies - needs to be PackageLoader" );
		foreach ( var package in StaticData.Loaded )
		{
			pl.LoadPackage( package.Package.FullIdent );
			Log.Info( $"Loaded package {package.Package.FullIdent} assemblies" );
		}
	}
}
