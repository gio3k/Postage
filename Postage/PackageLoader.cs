using System.Reflection;
using System.Text.Json;
using Sandbox;
using Sandbox.DataModel;

namespace Postage;

public static class PackageLoader
{
	private static void SetFileSystem( this PackageManager.ActivePackage pmx, BaseFileSystem fs )
	{
		var property =
			typeof(PackageManager.ActivePackage).GetProperty( "FileSystem",
				BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance );
		if ( property == null )
			throw new Exception( "Couldn't find ActivePackage.FileSystem" );
		property.SetValue( pmx, fs );
	}

	private static void SetPackage( this PackageManager.ActivePackage pmx, Package package )
	{
		var property =
			typeof(PackageManager.ActivePackage).GetProperty( "Package",
				BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance );
		if ( property == null )
			throw new Exception( "Couldn't find ActivePackage.Package" );
		property.SetValue( pmx, package );
	}

	internal static MemoryFileSystem CreateAssemblyFileSystem( IEnumerable<string> paths )
	{
		var mfs = new MemoryFileSystem();

		// Create folder
		mfs.CreateDirectory( "/.bin" );

		foreach ( var path in paths )
		{
			// Get file name at path
			Log.Info( $"Loading assembly {path}" );
			var fileName = Path.GetFileName( path );
			mfs.WriteAllBytes( fileName, File.ReadAllBytes( path ) );
		}

		return mfs;
	}

	private static void Mount( this PackageManager.ActivePackage pmx )
	{
		var method =
			typeof(PackageManager.ActivePackage).GetMethod( "Mount",
				BindingFlags.NonPublic | BindingFlags.Instance );
		if ( method == null )
			throw new Exception( "Couldn't find ActivePackage.Mount" );
		method.Invoke( pmx, null );
	}

	private static void AddLocalProject( LocalProject project )
	{
		var field = typeof(LocalProject).GetField( "All", BindingFlags.NonPublic | BindingFlags.Static );
		if ( field == null )
			throw new Exception( "Couldn't find LocalProject.All" );
		if ( field.GetValue( project ) is not List<LocalProject> all )
			throw new Exception( "Unknown type in LocalProject.All" );
		all.Add( project );
	}

	public static void LoadPackage( IEnumerable<string> assemblies, string path )
	{
		var project = new LocalProject { Path = Path.Combine( path, ".addon" ), Active = true };

		var text = File.ReadAllText( project.Path );
		project.Config = JsonSerializer.Deserialize<ProjectConfig>( text );
		project.Config.Init( project.Path );
		project.Config.HasCode = false;

		var package = project.Package;

		project.Active = true;

		AddLocalProject( project );

		var pmx = new PackageManager.ActivePackage();
		pmx.SetPackage( package );
		pmx.SetFileSystem( new AggFileSystem() );

		// Mount content path (if required)
		pmx.FileSystem.Mount( new RootFileSystem( path ) );

		// Create assembly fs
		pmx.FileSystem.Mount( CreateAssemblyFileSystem( assemblies ) );

		// Add to ActivePackages
		PackageManager.ActivePackages.Add( pmx );

		pmx.AddContextTag( "server" );

		// Mount!
		pmx.Mount();
	}

	public static void Init()
	{
		Log.Info( "Loading app package..." );

		var assemblies = Launcher.LaunchOptions.Libraries.ToList();
		assemblies.Add( Launcher.LaunchOptions.AppAssembly );

		LoadPackage( assemblies, Launcher.LaunchOptions.AppContent );
	}
}
