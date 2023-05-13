using System.Reflection;
using Sandbox;
using Sandbox.Engine;
using Sandbox.Internal;

namespace Postage;

public static class ServerInitialize
{
	private static MethodInfo GetOnPackageAssemblyLoaded( object instance )
	{
		var type = instance.GetType();
		var method = type.Method( "OnPackageAssemblyLoaded" );
		return method;
	}

	public static object InitializePackageLoader( object instance )
	{
		var packageLoader = new PackageLoader( GlobalGameNamespace.TypeLibrary, "game" );
		packageLoader.OnAssemblyLoaded += ( s, bytes ) =>
		{
			// yes, this is bad!
			GetOnPackageAssemblyLoaded( instance ).Invoke( instance, new object[] { s, bytes } );
		};

		try
		{
			Game.HotloadManager.AssemblyResolver = packageLoader.AccessControl;
		}
		catch ( Exception e )
		{
			Log.Warn( "Couldn't set AssemblyResolver?" );
		}

		return packageLoader;
	}

	public static void SetPackage( Project project )
	{
		if ( project.ActivePackage == null )
			throw new Exception( "Project has no active package!" );
		ServerContext.GamePackage = project.ActivePackage;
	}

	public static void LoadAssemblies( object loader )
	{
		foreach ( var project in ProjectManager.Projects )
		{
			ProjectManager.LoadProjectAssemblies( project, (PackageLoader)loader );
		}
	}
}
