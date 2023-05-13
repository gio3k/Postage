using Sandbox;
using Sandbox.Internal;

namespace Postage;

public static class ProjectManager
{
	/// <summary>
	/// PackageManager.ActivePackages as objects
	/// </summary>
	public static IEnumerable<object> ReflectionActiveProjects =>
		PackageManager.ActivePackages.Select( v => (object)v );

	internal static IEnumerable<PackageManager.ActivePackage> ActiveProjects => PackageManager.ActivePackages;

	public static List<Project> Projects { get; } = new();

	/// <summary>
	/// The main game project - needs to be set!
	/// </summary>
	public static Project GameProject;

	internal static void LoadProjectAssemblies( Project project, PackageLoader loader )
	{
		var apfs = project.ActivePackage.FileSystem;
		foreach ( var name in apfs.FindFile( "/.bin/", "*.dll", recursive: true ) )
		{
			// Load assembly into stream
			Log.Info( $"Loading assembly {name} from project {project}" );
			var bytes = apfs.ReadAllBytes( $"/.bin/{name}" ).ToArray();
			var stream = new MemoryStream( bytes );

			// Create registration
			var registration = new AssemblyRegistration();
			registration.Name = Path.GetFileNameWithoutExtension( name );
			registration.Force = true;

			// Load stream
			registration.DllStream = loader.AccessControl.TrustUnsafe( stream );

			// Create result
			var typeLibrary = (TypeLibrary)loader.GetType().Property( "TypeLibrary" ).GetValue( loader );
			var result = typeLibrary.AddAssembly( registration );

			// Invoke OnAssemblyLoaded
			loader.OnAssemblyLoaded?.Invoke( registration.Name, bytes );

			Log.Info( $"Loaded {name} - {result.AssemblyName}, success: {result.Success}" );
		}
	}
}
