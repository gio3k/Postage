using System.Text.Json;
using Sandbox;
using Sandbox.DataModel;

namespace Postage;

public static class ProjectCreator
{
	public static void MakeGameProject( Project project )
	{
		// Create LocalProject first
		var localProject = new LocalProject { Path = Path.Combine( project.AddonDirectory, ".addon" ), Active = true };

		// Read addon
		localProject.Config = JsonSerializer.Deserialize<ProjectConfig>( File.ReadAllText( localProject.Path ) );
		localProject.Config.Init( localProject.Path );
		localProject.Config.HasCode = false;
		localProject.Config.PackageReferences = null;
		localProject.Active = true;

		// Get / update the mock package
		var localPackage = localProject.Package;

		// Create the active package
		project.ActivePackage = new PackageManager.ActivePackage();
		project.ActivePackage.SetPackage( localPackage );
		var activePackageFs = new AggFileSystem();
		project.ActivePackage.SetFileSystem( activePackageFs );

		// Create the assembly file system
		if ( project.Assemblies != null )
		{
			var mfs = new MemoryFileSystem();
			mfs.CreateDirectory( "/.bin" );
			foreach ( var path in project.Assemblies )
				mfs.WriteAllBytes( $"/.bin/{Path.GetFileName( path )}", File.ReadAllBytes( path ) );

			// Mount it
			activePackageFs.Mount( mfs );
		}

		// Add to active packages
		PackageManager.ActivePackages.Add( project.ActivePackage );
		Log.Info( $"Loaded and mounted addon {project.AddonDirectory}" );
	}

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
}
