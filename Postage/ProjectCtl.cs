using System.Reflection;
using System.Text.Json;
using Sandbox;
using Sandbox.DataModel;

namespace Postage;

public static class ProjectCtl
{
	private static void SetFileSystem( this PackageManager.ActivePackage pmx, BaseFileSystem fs )
	{
		var property =
			typeof(PackageManager.ActivePackage).Property( "FileSystem" );
		if ( property == null )
			throw new Exception( "Couldn't find ActivePackage.FileSystem" );
		property.SetValue( pmx, fs );
	}

	private static void SetPackage( this PackageManager.ActivePackage pmx, Package package )
	{
		var property =
			typeof(PackageManager.ActivePackage).Property( "Package" );
		if ( property == null )
			throw new Exception( "Couldn't find ActivePackage.Package" );
		property.SetValue( pmx, package );
	}

	private static void AddLocalProject( LocalProject project )
	{
		var field = typeof(LocalProject).Field( "All" );
		if ( field == null )
			throw new Exception( "Couldn't find LocalProject.All" );
		if ( field.GetValue( project ) is not List<LocalProject> all )
			throw new Exception( "Unknown type in LocalProject.All" );
		all.Add( project );
	}

	internal static PackageManager.ActivePackage Load( IEnumerable<string> binaries, string path,
		List<string> references = null )
	{
		// Create a LocalProject first
		var project = new LocalProject { Path = Path.Combine( path, ".addon" ), Active = true };

		// Read addon!
		project.Config = JsonSerializer.Deserialize<ProjectConfig>( File.ReadAllText( project.Path ) );
		project.Config.Init( project.Path );
		project.Config.HasCode = false;
		project.Config.PackageReferences = references ?? new List<string>();
		project.Active = true;

		// Get / update the bart package or whatever it's called
		var package = project.Package;

		// Add project to static project list
		AddLocalProject( project );

		// Finally create the active package
		var pmx = new PackageManager.ActivePackage();
		pmx.SetPackage( package );
		pmx.SetFileSystem( new AggFileSystem() );

		// Create the assembly file system
		if ( binaries != null )
		{
			var afs = new MemoryFileSystem();
			afs.CreateDirectory( "/.bin" );
			foreach ( var binary in binaries )
				afs.WriteAllBytes( $"/.bin/{Path.GetFileName( binary )}", File.ReadAllBytes( binary ) );

			// Mount it
			pmx.FileSystem.Mount( afs );
		}

		// Finally add to ActivePackages
		PackageManager.ActivePackages.Add( pmx );

		Log.Info( $"Loaded and mounted addon {path}" );

		return pmx;
	}
}
